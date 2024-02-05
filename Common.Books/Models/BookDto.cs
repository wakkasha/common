namespace Common.Books.Models;

public class BookDto
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public List<PageDto> Pages { get; set; } = new();
}