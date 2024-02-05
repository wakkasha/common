using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Common.Books.Configs;
using Common.Books.Interfaces;

namespace Common.Books.Services;

public class BlobService : IBlobService
{
    private readonly BlobServiceClient _blobServiceClient;
    public BlobService(BooksBlobConfig booksBlobConfig)
    {
        _blobServiceClient = new(booksBlobConfig.BooksStorageConnectionString);
    }
    public async Task<List<string>> ListBlobsInContainerAsync(string containerName)
    {
        var blobContainerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        var blobNames = new List<string>();

        await foreach (var blobItem in blobContainerClient.FindBlobsByTagsAsync($"@container='{containerName}'"))
            blobNames.Add(blobItem.BlobName);

        return blobNames;
    }

    public async Task<MemoryStream?> GetBlobContentAsync(string containerName, string blobName)
    {
        var blobContainerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        var blobClient = blobContainerClient.GetBlobClient(blobName);

        if (!await blobClient.ExistsAsync()) // Check if the blob exists
            return null; // or throw an exception
        BlobDownloadInfo download = await blobClient.DownloadAsync();

        var ms = new MemoryStream();
        await download.Content.CopyToAsync(ms);
        ms.Position = 0; // Reset the stream's position to start before returning

        return ms;
    }

    public async Task<string> UploadAsync(string containerName, Stream stream, string fileName,
        CancellationToken cancellationToken = default)
    {
        var blobContainerClient = _blobServiceClient.GetBlobContainerClient(containerName);

        // Ensure the container exists. In a real-world scenario, you'd want to manage your containers differently.
        await blobContainerClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken);

        var blobClient = blobContainerClient.GetBlobClient(fileName);

        await blobClient.UploadAsync(stream, true, cancellationToken);

        return blobClient.Uri.ToString();
    }
}