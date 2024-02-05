using Common.Books.Data;
using Common.Books.Data.Entities;
using Common.Books.Data.Enums;
using Common.Books.Data.Models;
using Common.Books.Extensions;
using Common.Books.Interfaces;
using Serilog;

namespace Common.Books.Services;

public class PageService : IPageService
{
    private readonly BooksDbContext _dbContext;
    private readonly ILogger _logger;
    private readonly IKafkaService _kafkaService;
    private int _pageNumber = -1;
    public PageService(
        BooksDbContext dbContext,
        ILogger logger,
        IKafkaService kafkaService)
    {
        _dbContext = dbContext;
        _logger = logger;
        _kafkaService = kafkaService;
    }
public async Task<List<Page>> InitializePagesAsync(BookRequest bookRequest, Guid bookId, List<GptPage> gptPages)
    {
        var coverPrompt =
            $"Generate a cartoon art image with playful colors. {bookRequest.Name} is {bookRequest.Age} year old {bookRequest.Ethnicity} {bookRequest.Gender} and she likes the playground.";
        var pages = new List<Page>
        {
            GeneratePage(bookId, GetNextPageNumber(), "Cover", string.Empty, PageType.Cover, prompt: coverPrompt),
            GeneratePage(bookId, GetNextPageNumber(), "Welcome", string.Empty),
            GeneratePage(bookId, GetNextPageNumber(), string.Empty, string.Empty, PageType.Dedication)
        };

        var contentPages = GenerateContentPages(bookId, gptPages);
        pages.AddRange(contentPages);
        
        pages.Add(GeneratePage(bookId, GetNextPageNumber(), "Qr Code", string.Empty, PageType.QrCode));
        pages.Add(GeneratePage(bookId, GetNextPageNumber(), string.Empty, string.Empty, PageType.BackCover));
        _logger.Information("Pages added for {BookId}", bookId);

        await _dbContext.Pages.AddRangeAsync(pages);
        await _dbContext.SaveChangesAsync();

        await PublishPageEventsAsync(pages);
        return pages;
    }

    public async Task<List<Page>> AddPagesAsync(List<Page> pages)
    {
        _dbContext.Pages.AddRange(pages);
        await _dbContext.SaveChangesAsync();
        return pages;
    }

    public async Task<Page> AddPageAsync(Page page)
    {
        await _dbContext.Pages.AddAsync(page);
        await _dbContext.SaveChangesAsync();
        return page;
    }

    private int GetNextPageNumber()
    {
        return ++_pageNumber;
    }

    private List<Page> GenerateContentPages(Guid bookId, List<GptPage> gptPages)
    {
        var result = new List<Page>();
        foreach (var page in gptPages)
        {
            var contentTextPage =
                GeneratePage(
                    bookId,
                    GetNextPageNumber(),
                    page.Chapter,
                    page.Content,
                    PageType.ContentText);
            result.Add(contentTextPage);

            var contentImagePage =
                GeneratePage(bookId,
                    GetNextPageNumber(),
                    string.Empty,
                    "This is a placeholder image",
                    PageType.ContentImage,
                    isImage: true);
            result.Add(contentImagePage);
        }

        return result;
    }

    private static Page GeneratePage(
        Guid bookId,
        int pageNumber,
        string chapter,
        string content,
        PageType pageType = PageType.DefaultPage,
        string? imageUrl = null,
        string? pageUrl = null,
        string? prompt = null,
        bool isImage = false)
    {
        return new Page
        {
            PageNumber = pageNumber,
            PageType = pageType,
            IsImage = isImage,
            ImageUrl = imageUrl,
            PageUrl = pageUrl,
            Content = content,
            Chapter = chapter,
            BookId = bookId,
            Prompt = prompt,
            Status = PageStatus.Pending
        };
    }

    private async Task PublishPageEventsAsync(IReadOnlyCollection<Page> pages)
    {
        var pagesToPublish = pages.ToList();

        var coverPage = pagesToPublish.First(x => x.PageType == PageType.Cover);
        await _kafkaService.ProduceAsync(Constants.Kafka.Topics.ImageRequested, coverPage.Id.ToString(),
            coverPage.ToPageDto());

        var qrCodePage = pagesToPublish.First(x => x.PageType == PageType.QrCode);
        await _kafkaService.ProduceAsync(Constants.Kafka.Topics.PdfRequested, coverPage.Id.ToString(),
            qrCodePage.ToPageDto());
        
        var pdfTextPages = pagesToPublish.Where(x
                => x.PageType != PageType.Cover 
                   && x.PageType != PageType.ContentImage
                   && x.PageType != PageType.BackCover)
            .ToList();

        foreach (var page in pdfTextPages)
        {
            var pageDto = page.ToPageDto();
            await _kafkaService.ProduceAsync(Constants.Kafka.Topics.PdfRequested, page.Id.ToString(), pageDto);
        }

        var pdfImagePages = pagesToPublish
            .Where(x => x.PageType == PageType.ContentImage)
            .ToList();
        
        await Task.Run(async () =>
        {
            await Task.Delay(TimeSpan.FromSeconds(35)); // Delay for 35 seconds give the Cover time to generate so we can get a use that character
            
            foreach (var page in pdfImagePages)
            {
                var pageDto = page.ToPageDto();
                await _kafkaService.ProduceAsync(Constants.Kafka.Topics.ImageRequested, page.Id.ToString(), pageDto);
            }
        });
        
    }
}