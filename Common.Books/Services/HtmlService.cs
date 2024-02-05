using System.Text;
using Common.Books.Interfaces;

namespace Common.Books.Services;

public class HtmlService : IHtmlService
{
    private readonly IBlobService _blobService;

    public HtmlService(IBlobService blobService)
    {
        _blobService = blobService;
    }
    public async Task<string> GetAndApplyReplacementsAsync(
        string templateName,
        Dictionary<string, string> data
    )
    {
        var blobStream = await _blobService.GetBlobContentAsync(
            "html",
            $"templates/{templateName}.html"
        );
        if (blobStream == null)
            throw new FileNotFoundException($"{templateName} Template file not found");

        // Ensure the stream position is set to the beginning
        blobStream.Position = 0;

        string result;
        using (var reader = new StreamReader(blobStream, Encoding.UTF8))
        {
            result = await reader.ReadToEndAsync();
        }

        foreach (var (key, value) in data)
            result = result.Replace("{{" + key + "}}", value);

        return result;
    }
}