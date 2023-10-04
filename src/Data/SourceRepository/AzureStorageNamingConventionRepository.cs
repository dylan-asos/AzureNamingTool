using System.Text;
using System.Text.Json;
using Azure.Storage.Blobs;

namespace AzureNamingTool.Data.SourceRepository;

public class AzureStorageNamingConventionRepository : INamingConventionRepository
{
    private const string BlobContainerName = "aznamingtool";

    private readonly BlobServiceClient _blobServiceClient;

    internal AzureStorageNamingConventionRepository(BlobServiceClient blobServiceClient)
    {
        _blobServiceClient = blobServiceClient;
    }

    public async Task<string> ReadData(string fileName)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(BlobContainerName);

        var blobClient = containerClient.GetBlobClient(fileName);
        if (!await blobClient.ExistsAsync())
        {
            return string.Empty;
        }

        using var ms = new MemoryStream();
        await blobClient.DownloadToAsync(ms);
        var blobStream = await blobClient.OpenReadAsync();

        using var streamReader = new StreamReader(blobStream);
        var resultContent = await streamReader.ReadToEndAsync();

        return resultContent;
    }

    public async Task WriteData(string fileName, object data)
    {
        var serializedData = JsonSerializer.Serialize(data);

        var containerClient = _blobServiceClient.GetBlobContainerClient(BlobContainerName);
        var blobClient = containerClient.GetBlobClient(fileName);

        await blobClient.UploadAsync(new MemoryStream(Encoding.UTF8.GetBytes(serializedData)), overwrite: true);
    }
}