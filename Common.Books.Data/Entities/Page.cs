using Common.Books.Data.Enums;

namespace Common.Books.Data.Entities;

public class Page : BaseEntity
{
    public int PageNumber { get; set; }
    public bool IsImage { get; set; }
    public string? ImageUrl { get; set; }
    public string? PageUrl { get; set; }
    public string? Content { get; set; }
    public string? Chapter { get; set; }
    public string? Prompt { get; set; }
    public bool IsDeleted { get; set; }
    public int Version { get; set; }
    public PageType PageType { get; set; }
    public Guid BookId { get; set; }
    public Book? Book { get; set; }
    public PageStatus Status { get; set; }
    public string Test { get; set; }
}