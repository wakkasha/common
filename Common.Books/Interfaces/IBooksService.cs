using Common.Books.Models;

namespace Common.Books.Interfaces;

public interface IBooksService
{
    Task<Guid> GenerateBookAsync(Guid bookRequestId);
    Task<BookDto> GetByIdAsync(Guid bookId, bool includePages = false);
    Task<bool> UserApproveAsync(Guid lookupId);
}