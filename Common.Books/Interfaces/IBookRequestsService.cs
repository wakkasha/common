using Common.Books.Data.Models;
using Common.Books.Models;

namespace Common.Books.Interfaces;

public interface IBookRequestsService
{
    Task<bool> UpdatePaymentIntentAsync(Guid bookRequestId, string paymentIntentId);
    Task<BookRequestResponseDto> StartAsync(BookRequestDto bookRequestDto);
    Task<GPTBookResponse> PreviewAsync(Guid previewId);
    Task<bool> ConfirmBookPreviewAsync(Guid previewId);
    Task MergeAsync(Guid bookId);
}