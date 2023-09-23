using System.Text.Json;
using AzureNamingTool.Models;

namespace AzureNamingTool.Helpers;

public class FileReader
{
    private readonly CacheHelper _cacheHelper;
    private readonly FileSystemHelper _fileSystemHelper;

    public FileReader(CacheHelper cacheHelper, FileSystemHelper fileSystemHelper)
    {
        _cacheHelper = cacheHelper;
        _fileSystemHelper = fileSystemHelper;
    }
    
    public List<T>? GetList<T>()
    {
        var items = new List<T>();

        // Check if the data is cached
        var data = (string) _cacheHelper.GetCacheObject(typeof(T).Name)!;
        
        // Load the data from the file system.
        if (string.IsNullOrEmpty(data))
        {
            data = typeof(T).Name switch
            {
                nameof(ResourceComponent) => _fileSystemHelper.ReadFile(FileNames.ResourceComponent),
                nameof(ResourceEnvironment) => _fileSystemHelper.ReadFile(FileNames.ResourceEnvironment),
                nameof(ResourceLocation) => _fileSystemHelper.ReadFile(FileNames.ResourceLocation),
                nameof(ResourceOrg) => _fileSystemHelper.ReadFile(FileNames.ResourceOrg),
                nameof(ResourceProjAppSvc) => _fileSystemHelper.ReadFile(FileNames.ResourceProjAppSvc),
                nameof(ResourceType) => _fileSystemHelper.ReadFile(FileNames.ResourceType),
                nameof(ResourceUnitDept) => _fileSystemHelper.ReadFile(FileNames.ResourceUnitDept),
                nameof(ResourceFunction) => _fileSystemHelper.ReadFile(FileNames.ResourceFunction),
                nameof(ResourceDelimiter) => _fileSystemHelper.ReadFile(FileNames.ResourceDelimiter),
                nameof(CustomComponent) => _fileSystemHelper.ReadFile(FileNames.CustomComponent),
                nameof(AdminLogMessage) => _fileSystemHelper.ReadFile(FileNames.AdminLogMessage),
                nameof(GeneratedName) => _fileSystemHelper.ReadFile(FileNames.GeneratedName),
                nameof(AdminUser) => _fileSystemHelper.ReadFile("adminusers.json"),
                _ => "[]"
            };
            _cacheHelper.SetCacheObject(typeof(T).Name, data);
        }

        if (data != "[]")
        {
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true
            };

            items = JsonSerializer.Deserialize<List<T>>(data, options);
        }
        
        return items;
    }
}