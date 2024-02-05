using Common.Books.Data;
using Common.Books.Data.Entities;
using Common.Books.Data.Enums;
using Common.Books.Interfaces;
using Common.Books.Models;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Common.Books.Services;

public class BooksService : IBooksService
{
    private readonly BooksDbContext _booksDb;
    private readonly IPageService _pageService;
    private readonly ILogger _logger;

    public BooksService(BooksDbContext booksDb, IPageService pageService, ILogger logger)
    {
        _booksDb = booksDb;
        _pageService = pageService;
        _logger = logger;
    }
    public async Task<Guid> GenerateBookAsync(Guid bookRequestId)
    {
        var bookRequest = await _booksDb
            .BookRequests
            .Include(x => x.BookPreviews)
            .ThenInclude(bookPreview => bookPreview.PromptResponse)
            .ThenInclude(gptBookResponse => gptBookResponse.Pages)
            .FirstOrDefaultAsync(x => x.Id == bookRequestId
                                      && x.PaymentIntentId != null);

        if (bookRequest == null)
        {
            _logger.Error("Book Request not found {@BookRequestId}", bookRequestId);
            throw new Exception("Book Request not found");
        }

        var bookPreview = bookRequest.BookPreviews
            .Where(x => x.BookRequestId == bookRequestId && x.PreviewStatus == PreviewStatus.Selected)
            .OrderBy(x => x.CreatedDate)
            .First();

        var existingBook = await _booksDb.Books.FirstOrDefaultAsync(x => x.BookRequestId == bookRequestId);
        if (existingBook is {Status: BookStatus.TextGenerated})
        {
            _logger.Information("Book already exists for {@BookRequestId}", bookRequestId);
            return existingBook.Id;
        }

        var book = GenerateBookEntity(bookRequest, bookPreview);
        var bookAdded = await _booksDb.Books.AddAsync(book);
        await _booksDb.SaveChangesAsync();
        var bookId = bookAdded.Entity.Id;
        _logger.Information("Found the requested book, BookId {@BookId}", bookRequest.Id);

        try
        {
            var response = bookPreview.PromptResponse;
            await _pageService.InitializePagesAsync(bookRequest, bookId, response.Pages);

            var bookUpdated = await _booksDb.Books.FindAsync(bookId);
            if (bookUpdated != null)
            {
                bookUpdated.Title = response.Title;
                bookUpdated.Status = BookStatus.TextGenerated;
                bookUpdated.UpdatedDate = DateTime.UtcNow;
                bookUpdated.UpdatedBy = "BooksService";
                _booksDb.Books.Update(bookUpdated);
            }

            await _booksDb.SaveChangesAsync();
            return bookUpdated!.Id;
        }
        catch (Exception e)
        {
            _logger.Error(e, "Book generation failed {@BookId}", bookId);
            throw;
        }
    }

    public async Task<BookDto> GetByIdAsync(Guid bookId, bool includePages = false)
    {
        var book = _booksDb.Books.Where(x => x.Id == bookId);
        if (includePages)
            book = book.Include(x => x.Pages);

        var result = await book.Include(book => book.Pages).FirstOrDefaultAsync();
        if (result == null)
            return null!;

        var bookDto = new BookDto
        {
            Title = result.Title ?? "",
            Pages = result.Pages
                .OrderBy(x => x.PageNumber)
                .Select(x => new PageDto
                {
                    Id = x.Id,
                    Chapter = x.Chapter,
                    Content = x.Content,
                    PageType = x.PageType,
                    Prompt = x.Prompt,
                    PageNumber = x.PageNumber,
                    ImageUrl = x.ImageUrl,
                    PageUrl = x.PageUrl
                })
                .ToList()
        };
        return bookDto;
    }

    public async Task<bool> UserApproveAsync(Guid lookupId)
    {
        var book = await _booksDb.Books.FirstOrDefaultAsync(x => x.LookupId == lookupId);
        if (book == null)
            return false;

        book.Status = BookStatus.UserApproved;
        book.UpdatedDate = DateTime.UtcNow;
        book.UpdatedBy = "User";
        _booksDb.Books.Update(book);
        await _booksDb.SaveChangesAsync();
        return true;
    }

    private static Book GenerateBookEntity(BookRequest bookRequest, BookPreview preview)
    {
        var book = new Book
        {
            Title = "Generating...",
            Status = BookStatus.Requested,
            Category = BookCategory.Kids,
            BookRequest = bookRequest,
            BookPreview = preview,
            BookPreviewId = preview.Id,
            BookRequestId = bookRequest.Id
        };
        return book;
    }
}