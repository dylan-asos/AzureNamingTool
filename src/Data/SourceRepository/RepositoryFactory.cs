using Azure.Core;
using Azure.Storage.Blobs;
using AzureNamingTool.Helpers;
using AzureNamingTool.Models;

namespace AzureNamingTool.Data.SourceRepository;

public class RepositoryFactory
{
    private readonly SiteConfiguration _siteConfiguration;
    private readonly FileSystemHelper _fileSystemHelper;
    private readonly BlobServiceClient _blobServiceClient;

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
            "CosmosDb" => new CosmosNamingConventionRepository(),
            "AzureStorage" => new AzureStorageNamingConventionRepository(_blobServiceClient),
            _ => new FileBasedNamingConventionRepository(_fileSystemHelper)
        };
    }
}