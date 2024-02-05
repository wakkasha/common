using Common.Books.Data.Enums;
using Common.Books.Data.Models;

namespace Common.Books.Data.Entities;

public class BookPreview : BaseEntity
{
    public Guid BookRequestId { get; set; }
    public BookRequest? BookRequest { get; set; }
    public required string BookTitle { get; set; }
    public required string Prompt { get; set; }
    public required GPTBookResponse PromptResponse { get; set; }
    public PreviewStatus PreviewStatus { get; set; }
}