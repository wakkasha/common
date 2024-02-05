using Common.Books.Data.Enums;

namespace Common.Books.Data.Entities;

public class BookRequest : BaseEntity
{
    public required string Username { get; set; }
    public string? PaymentIntentId { get; set; }
    public int Age { get; set; }
    public required string Name { get; set; }
    public required string Gender { get; set; }
    public required string FavoriteColor { get; set; }
    public required string Ethnicity { get; set; }
    public BookRequestStatus BookRequestStatus { get; set; }
    public virtual ICollection<BookPreview> BookPreviews { get; set; } = new List<BookPreview>();
}