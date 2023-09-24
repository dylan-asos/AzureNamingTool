using AzureNamingTool.Data.SourceRepository;
using AzureNamingTool.Models;

namespace AzureNamingTool.Helpers;

public class FileWriter
{
    private readonly CacheHelper _cacheHelper;
    private readonly RepositoryFactory _repositoryFactory;

    public FileWriter(
        CacheHelper cacheHelper, 
        RepositoryFactory repositoryFactory)
    {
        _cacheHelper = cacheHelper;
        _repositoryFactory = repositoryFactory;
    }

    public async Task WriteList<T>(List<T> items)
    {
        var targetRepository = _repositoryFactory.GetRepository();

        var fileName = typeof(T).Name switch
        {
            nameof(ResourceComponent) => FileNames.ResourceComponent,
            nameof(ResourceEnvironment) => FileNames.ResourceEnvironment,
            nameof(ResourceLocation) => FileNames.ResourceLocation,
            nameof(ResourceOrg) => FileNames.ResourceOrg,
            nameof(ResourceProjAppSvc) => FileNames.ResourceProjAppSvc,
            nameof(ResourceType) => FileNames.ResourceType,
            nameof(ResourceUnitDept) => FileNames.ResourceUnitDept,
            nameof(ResourceFunction) => FileNames.ResourceFunction,
            nameof(ResourceDelimiter) => FileNames.ResourceDelimiter,
            nameof(CustomComponent) => FileNames.CustomComponent,
            nameof(AdminLogMessage) => FileNames.AdminLogMessage,
            nameof(GeneratedName) => FileNames.GeneratedName,
            nameof(AdminUser) => FileNames.AdminUser,
            _ => throw new ArgumentOutOfRangeException()
        };

        await targetRepository.WriteData(fileName, items);
        var data = await targetRepository.ReadData(fileName);
        
        _cacheHelper.SetCacheObject(typeof(T).Name, data);
    }
}