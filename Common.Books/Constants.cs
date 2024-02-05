namespace Common.Books;

public class Constants
{
    public static class Kafka
    {
        public static class Topics
        {
            public const string ImageRequested = "image-requested";
            public const string PagesRequested = "pages-requested";
            public const string PdfRequested = "pdf-requested";
            public const string PdfBookRequested = "pdf-book-requested";
        }
    }
}