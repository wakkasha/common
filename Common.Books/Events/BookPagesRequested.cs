namespace Common.Books.Events;

public class BookPagesRequested
{
    public Guid BookRequestId { get; set; }
    public string Username { get; set; }
}