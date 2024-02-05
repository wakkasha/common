namespace Common.Books.Interfaces;

public interface IBlobService
{
    Task<string> UploadAsync(string containerName, Stream stream, string fileName,
        CancellationToken cancellationToken = default);

    Task<List<string>> ListBlobsInContainerAsync(string containerName);
    Task<MemoryStream?> GetBlobContentAsync(string containerName, string blobName);
}