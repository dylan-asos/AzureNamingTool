using AzureNamingTool.Helpers;
using AzureNamingTool.Models;

namespace AzureNamingTool.Data.SourceRepository;

public class RepositoryFactory
{
    private readonly SiteConfiguration _siteConfiguration;
    private readonly FileSystemHelper _fileSystemHelper;

    public RepositoryFactory(
        SiteConfiguration siteConfiguration, 
        FileSystemHelper fileSystemHelper)
    {
        _siteConfiguration = siteConfiguration;
        _fileSystemHelper = fileSystemHelper;
    }
    
    public INamingConventionRepository GetRepository()
    {
        var repositoryType = _siteConfiguration.RepositoryType;
        
        return repositoryType switch
        {
            "File" => new FileBasedNamingConventionRepository(_fileSystemHelper),
            "CosmosDb" => new CosmosNamingConventionRepository(),
            "AzureStorage" => new AzureStorageNamingConventionRepository(),
            _ => new FileBasedNamingConventionRepository(_fileSystemHelper)
        };
    }
}