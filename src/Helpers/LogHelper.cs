using System.Text.Json;
using AzureNamingTool.Models;

namespace AzureNamingTool.Helpers;

public class LogHelper
{
    private readonly CacheHelper _cacheHelper;
    private readonly FileSystemHelper _fileSystemHelper;

    public LogHelper(FileSystemHelper fileSystemHelper, CacheHelper cacheHelper)
    {
        _fileSystemHelper = fileSystemHelper;
        _cacheHelper = cacheHelper;
    }

    /// <summary>
    ///     This function prugres the generated names log.
    /// </summary>
    /// <returns>void</returns>
    public void PurgeGeneratedNames()
    {
        _fileSystemHelper.WriteFile(FileNames.GeneratedName, "[]");
        _cacheHelper.InvalidateCacheObject("GeneratedName");
    }

    /// <summary>
    ///     This function returns the Admin log.
    /// </summary>
    /// <returns>List of AdminLogMessages - List of Adming Log messages.</returns>
    private List<AdminLogMessage> GetAdminLog()
    {
        var data = _fileSystemHelper.ReadFile(FileNames.AdminLogMessage);

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };

        var lstAdminLogMessages =
            JsonSerializer.Deserialize<List<AdminLogMessage>>(data, options)!
                .OrderByDescending(x => x.CreatedOn)
                .ToList();

        return lstAdminLogMessages;
    }

    /// <summary>
    ///     This function purges the Admin log.
    /// </summary>
    /// <returns>void</returns>
    public void PurgeAdminLog()
    {
        _fileSystemHelper.WriteFile(FileNames.AdminLogMessage, "[]");
        _cacheHelper.InvalidateCacheObject("AdminLogMessage");
    }
}