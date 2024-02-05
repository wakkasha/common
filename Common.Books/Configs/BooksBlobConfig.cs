namespace Common.Books.Configs;

public class BooksBlobConfig : KvConfig
{
    public string BooksStorageConnectionString { get; set; }
    public string BooksStorageContainerName { get; set; }
}