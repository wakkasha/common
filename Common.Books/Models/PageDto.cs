using Common.Books.Data.Enums;

namespace Common.Books.Models;

public class PageDto
{
    public Guid Id { get; set; }
    public Guid BookId { get; set; }
    public int PageNumber { get; set; }
    public string? Content { get; set; }
    public string? Chapter { get; set; }
    public PageType PageType { get; set; }
    public string? Prompt { get; set; }
    public string? PageUrl { get; set; }
    public string? ImageUrl { get; set; }
    public Dictionary<string, string> MetaData { get; set; } = new();
}