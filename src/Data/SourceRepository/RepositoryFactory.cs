using Azure.Storage.Blobs;
using AzureNamingTool.Helpers;
using AzureNamingTool.Models;

namespace AzureNamingTool.Data.SourceRepository;

public class RepositoryFactory
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly FileSystemHelper _fileSystemHelper;
    private readonly SiteConfiguration _siteConfiguration;

    public RepositoryFactory(
        SiteConfiguration siteConfiguration,
        FileSystemHelper fileSystemHelper,
        BlobServiceClient blobServiceClient)
    {
        _siteConfiguration = siteConfiguration;
        _fileSystemHelper = fileSystemHelper;
        _blobServiceClient = blobServiceClient;
    }

    public INamingConventionRepository GetRepository()
    {
        var repositoryType = _siteConfiguration.RepositoryType;

        return repositoryType switch
        {
            "File" => new FileBasedNamingConventionRepository(_fileSystemHelper),
            "AzureStorage" => new AzureStorageNamingConventionRepository(_blobServiceClient),
            _ => new FileBasedNamingConventionRepository(_fileSystemHelper)
        };
    }
}