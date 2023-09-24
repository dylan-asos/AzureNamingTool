using System.Text.Json;
using AzureNamingTool.Data.SourceRepository;
using AzureNamingTool.Models;

namespace AzureNamingTool.Helpers;

public class FileReader
{
    private readonly CacheHelper _cacheHelper;
    private readonly RepositoryFactory _repositoryFactory;

    public FileReader(CacheHelper cacheHelper, RepositoryFactory repositoryFactory)
    {
        _cacheHelper = cacheHelper;
        _repositoryFactory = repositoryFactory;
    }
    
    public async Task<List<T>> GetList<T>()
    {
        var items = new List<T>();

        // Check if the data is cached
        var data = (string) _cacheHelper.GetCacheObject(typeof(T).Name)!;
        
        var targetRepository = _repositoryFactory.GetRepository();
        
        // Load the data from the file system.
        if (string.IsNullOrEmpty(data))
        {
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
                nameof(AdminUser) => FileNames.AdminUser
            };

            data = await targetRepository.ReadData(fileName);
            _cacheHelper.SetCacheObject(typeof(T).Name, data);
        }

        if (data == "[]") 
            return items;
        
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };

        return JsonSerializer.Deserialize<List<T>>(data, options) ?? new List<T>();
    }
}