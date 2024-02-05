using Common.Books.Data.Enums;

namespace Common.Books.Data.Entities;

public class Book : BaseEntity
{
    public string? Title { get; set; }
    public Guid BookPreviewId { get; set; }
    public BookPreview? BookPreview { get; set; }
    public string? GptResponse { get; set; }
    public string? BookUrl { get; set; }
    public Guid BookRequestId { get; set; }
    public required BookRequest? BookRequest { get; set; }
    public Guid LookupId { get; set; } = Guid.NewGuid();
    public BookStatus Status { get; set; }
    public BookCategory Category { get; set; }
    public bool IsDeleted { get; set; }
    public virtual ICollection<Page> Pages { get; set; } = new List<Page>();
}