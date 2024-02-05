using Common.Books.Data.Entities;
using Common.Books.Interfaces;
using Microsoft.Playwright;
using PdfSharpCore.Pdf;
using PdfSharpCore.Pdf.IO;

namespace Common.Books.Services;

public class PdfService : IPdfService
{
    private readonly IBlobService _blobService;

    public PdfService(IBlobService blobService)
    {
        _blobService = blobService;
    }
    public async Task<MemoryStream> HtmlToPdfStreamAsync(string html)
    {
        using var playwright = await Playwright.CreateAsync();
        var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true // Run browser in headless mode
        });

        var page = await browser.NewPageAsync();

        //await page.GotoAsync("file:///path/to/your/htmlfile.html"); // or http://...
        // For direct HTML content, use:
        await page.SetContentAsync(html);
        
        var pdfBytes = await page.PdfAsync(new PagePdfOptions
        {
            Format = "A4",
            Landscape = true
        });

        await browser.CloseAsync();
        return new MemoryStream(pdfBytes);
    }
    
    public async Task<MemoryStream> CreateMergedPdfAsync(IEnumerable<Page> entities)
    {
        var pdfStreams = new List<Stream>();

        foreach (var entity in entities)
        {
            var blobStream = await _blobService.GetBlobContentAsync("pdfs", $"{entity.BookId}/{entity.Id}.pdf");
            if (blobStream != null) pdfStreams.Add(blobStream);
        }

        var mergedPdfStream = MergePdf(pdfStreams);

        // Clean up individual streams after merging
        foreach (var stream in pdfStreams) 
            await stream.DisposeAsync();

        return mergedPdfStream;
    }
    
    private static MemoryStream MergePdf(IEnumerable<Stream> pdfStreams)
    {
        using var outputDocument = new PdfDocument();

        foreach (var pdfStream in pdfStreams)
        {
            pdfStream.Position = 0; // Ensure the stream is at the beginning
            using var inputDocument = PdfReader.Open(pdfStream, PdfDocumentOpenMode.Import);

            foreach (var page in inputDocument.Pages) 
                outputDocument.AddPage(page);
        }

        var outputStream = new MemoryStream();
        outputDocument.Save(outputStream, false);
        outputStream.Position = 0; // Reset the position for reading
        return outputStream;
    }
}