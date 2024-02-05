using Common.Books.Data.Entities;
using Common.Books.Data.Models;

namespace Common.Books.Interfaces;

public interface IPageService
{
    Task<List<Page>> InitializePagesAsync(BookRequest bookRequest, Guid bookId, List<GptPage> gptPages);
    Task<List<Page>> AddPagesAsync(List<Page> pages);

    Task<Page> AddPageAsync(Page page);
}