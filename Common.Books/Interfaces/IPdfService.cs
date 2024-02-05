using Common.Books.Data.Entities;

namespace Common.Books.Interfaces;

public interface IPdfService
{
    Task<MemoryStream> HtmlToPdfStreamAsync(string html);
    Task<MemoryStream> CreateMergedPdfAsync(IEnumerable<Page> entities);
}