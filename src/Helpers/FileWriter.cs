using AzureNamingTool.Models;

namespace AzureNamingTool.Helpers;

public class FileWriter
{
    private readonly CacheHelper _cacheHelper;
    private readonly FileSystemHelper _fileSystemHelper;

    public FileWriter(CacheHelper cacheHelper, FileSystemHelper fileSystemHelper)
    {
        _cacheHelper = cacheHelper;
        _fileSystemHelper = fileSystemHelper;
    }
    
    public void WriteList<T>(List<T> items)
    {
        switch (typeof(T).Name)
        {
            case nameof(ResourceComponent):
                _fileSystemHelper.WriteConfiguration(items, FileNames.ResourceComponent);
                break;
            case nameof(ResourceEnvironment):
                _fileSystemHelper.WriteConfiguration(items, FileNames.ResourceEnvironment);
                break;
            case nameof(ResourceLocation):
                _fileSystemHelper.WriteConfiguration(items, FileNames.ResourceLocation);
                break;
            case nameof(ResourceOrg):
                _fileSystemHelper.WriteConfiguration(items, FileNames.ResourceOrg);
                break;
            case nameof(ResourceProjAppSvc):
                _fileSystemHelper.WriteConfiguration(items, FileNames.ResourceProjAppSvc);
                break;
            case nameof(ResourceType):
                _fileSystemHelper.WriteConfiguration(items, FileNames.ResourceType);
                break;
            case nameof(ResourceUnitDept):
                _fileSystemHelper.WriteConfiguration(items, FileNames.ResourceUnitDept);
                break;
            case nameof(ResourceFunction):
                _fileSystemHelper.WriteConfiguration(items, FileNames.ResourceFunction);
                break;
            case nameof(ResourceDelimiter):
                _fileSystemHelper.WriteConfiguration(items, FileNames.ResourceDelimiter);
                break;
            case nameof(CustomComponent):
                _fileSystemHelper.WriteConfiguration(items, FileNames.CustomComponent);
                break;
            case nameof(AdminLogMessage):
                _fileSystemHelper.WriteConfiguration(items, FileNames.AdminLogMessage);
                break;
            case nameof(GeneratedName):
                _fileSystemHelper.WriteConfiguration(items, FileNames.GeneratedName);
                break;
            case nameof(AdminUser):
                _fileSystemHelper.WriteConfiguration(items, FileNames.AdminUser);
                break;
        }

        var data = typeof(T).Name switch
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
            nameof(AdminUser) => _fileSystemHelper.ReadFile(FileNames.AdminUser),
            _ => "[]"
        };

        // Update the cache with the latest data
        _cacheHelper.SetCacheObject(typeof(T).Name, data);
    }
}