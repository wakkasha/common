using System.Text;
using System.Text.RegularExpressions;
using Common.Books.Data;
using Common.Books.Data.Entities;
using Common.Books.Data.Enums;
using Common.Books.Data.Models;
using Common.Books.Extensions;
using Common.Books.Interfaces;
using Common.Books.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using OpenAI_API;
using OpenAI_API.Chat;
using OpenAI_API.Models;
using Serilog;

namespace Common.Books.Services;

public class BookRequestsService : IBookRequestsService
{
    private readonly BooksDbContext _booksDb;
    private readonly ILogger _logger;
    private readonly OpenAIAPI _chatGpt;
    private readonly IPdfService _pdfService;
    private readonly IBlobService _blobService;

    public BookRequestsService(BooksDbContext booksDb, ILogger logger, OpenAIAPI chatGpt, IPdfService pdfService,
        IBlobService blobService)
    {
        _booksDb = booksDb;
        _logger = logger;
        _chatGpt = chatGpt;
        _pdfService = pdfService;
        _blobService = blobService;
    }

    private static string? ExtractJson(string input)
    {
        // Regular expression to find a JSON object in the string
        var jsonRegex = new Regex(@"\{.*\}");

        // Find matches in the input string
        var match = jsonRegex.Match(input);

        return match.Success ? match.Value : null;
    }

    public async Task<BookRequestResponseDto> StartAsync(BookRequestDto bookRequestDto)
    {
        var prompt = RequestBuilder(bookRequestDto);
        var completionRequest = GetCompletionRequest(prompt);
        var result = await _chatGpt.Chat.CreateChatCompletionAsync(completionRequest);

        var outputResult = result.Choices.First().Message.Content;
        _logger.Information("GPT Response {@OutputResult}", outputResult);
        // var json = ExtractJson(outputResult);
        // if (json == null)
        // {
        //     _logger.Warning("No JSON found in GPT response {@OutputResult}", outputResult);
        //     throw new Exception("No JSON found in GPT response");
        // }

        var response = JsonConvert.DeserializeObject<GPTBookResponse>(outputResult);

        var bookRequest = new BookRequest
        {
            Username = bookRequestDto.RequestedBy,
            Age = bookRequestDto.Age,
            Name = bookRequestDto.Name,
            Gender = bookRequestDto.Gender,
            FavoriteColor = bookRequestDto.FavoriteColor,
            Ethnicity = bookRequestDto.Ethnicity,
            BookRequestStatus = BookRequestStatus.New
        };
        var bookRequestAdded = await _booksDb.BookRequests.AddAsync(bookRequest);
        var bookRequestId = bookRequestAdded.Entity.Id;
        var bookPreview = new BookPreview
        {
            BookRequestId = bookRequestId,
            BookTitle = response.Title,
            Prompt = prompt,
            PromptResponse = response,
            PreviewStatus = PreviewStatus.Generated
        };

        var bookPreviewEntry = await _booksDb.BookPreviews.AddAsync(bookPreview);
        await _booksDb.SaveChangesAsync();

        _logger.Information("Book Requested added {@BookRequestId}", bookRequest.Id);

        return new BookRequestResponseDto
        {
            BookRequestId = bookRequestId,
            PreviewId = bookPreviewEntry.Entity.Id
        };
    }

    public async Task<GPTBookResponse> PreviewAsync(Guid previewId)
    {
        var bookPreview = await _booksDb.BookPreviews
            .FirstAsync(x => x.Id == previewId);
        return bookPreview.PromptResponse;
    }

    public async Task<bool> ConfirmBookPreviewAsync(Guid previewId)
    {
        var bookPreview = await _booksDb.BookPreviews.FindAsync(previewId);
        if (bookPreview is null) throw new Exception("Book preview not found");

        bookPreview.PreviewStatus = PreviewStatus.Selected;
        return await _booksDb.SaveChangesAsync() > 0;
    }

    public async Task<bool> UpdatePaymentIntentAsync(Guid bookRequestId, string paymentIntentId)
    {
        var bookRequest = await _booksDb.BookRequests.FindAsync(bookRequestId);
        if (bookRequest == null)
        {
            _logger.Error("BookRequest not found {@BookRequestId}", bookRequestId);
            return false;
        }

        bookRequest.PaymentIntentId = paymentIntentId;
        _booksDb.BookRequests.Update(bookRequest);
        await _booksDb.SaveChangesAsync();
        return true;
    }

    public async Task MergeAsync(Guid bookId)
    {
        var book = await _booksDb
            .Books
            .Include(x => x.Pages)
            .FirstOrDefaultAsync(x => x.Id == bookId);

        var pages = book.Pages.OrderBy(x => x.PageNumber).ToList();
        var pdfFinal = await _pdfService.CreateMergedPdfAsync(pages);
        var bookTitle = book.Title.Replace(" ", "-");

        pdfFinal.Position = 0; // Reset the stream position before reading
        var url = await _blobService.UploadAsync("books", pdfFinal, $"{book.Id}/{bookTitle}.pdf");

        book.BookUrl = url;
        book.Status = BookStatus.PdfGenerated;
        book.UpdatedDate = DateTime.UtcNow;
        book.UpdatedBy = "PdfService";

        await _booksDb.SaveChangesAsync();
    }

    private static string RequestBuilder(BookRequestDto bookRequest)
    {
        var request = new StringBuilder();
        request.AppendWithNewLine($"Can you write a book for a {GetAgeCategory(bookRequest.Age)}");
        request.AppendWithNewLine($"Name {bookRequest.Name}");

        request.AppendWithNewLine($"they are a {bookRequest.Ethnicity} {bookRequest.Gender}");

        request.AppendWithNewLine($"they're {bookRequest.Age} year old");
        request.AppendWithNewLine($"And loves the color {bookRequest.FavoriteColor}");

        request.AppendWithNewLine(@"Can you format the response in JSON? I want it to look like this 
            {
                ""title"": """",
                ""pages"": [
                    {""chapter"": """", ""pageNumber"": ""1"", ""content"": """"},
                    {""chapter"": """", ""pageNumber"": ""2"", ""content"": """"}
                ]
            }
        ");

        request.AppendWithNewLine(
            $"Minimum 8 pages, maximum 15 pages. enough content on a page for a {GetAgeCategory(bookRequest.Age)} attention span");

        return request.ToString();
    }

    private static string GetAgeCategory(int age)
    {
        return age switch
        {
            < 5 => "baby",
            < 10 => "child",
            < 13 => "preteen",
            < 18 => "teenager",
            < 30 => "young adult",
            < 50 => "adult",
            _ => string.Empty
        };
    }

    private static ChatRequest GetCompletionRequest(string bookRequestDetails)
    {
        return new ChatRequest()
        {
            Model = Model.GPT4,
            Temperature = 0.5,
            MaxTokens = 1000,

            Messages = new ChatMessage[]
            {
                new(ChatMessageRole.User, bookRequestDetails)
            }
        };
    }
}