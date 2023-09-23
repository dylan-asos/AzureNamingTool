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
        List<AdminLogMessage> lstAdminLogMessages = new();

        var data = _fileSystemHelper.ReadFile(FileNames.AdminLogMessage);
        
        if (data != null)
        {
            var items = new List<AdminLogMessage>();
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true
            };
            lstAdminLogMessages = JsonSerializer.Deserialize<List<AdminLogMessage>>(data, options)!
                .OrderByDescending(x => x.CreatedOn).ToList();
        }

        return lstAdminLogMessages;
    }

    /// <summary>
    ///     This function logs the Admin message.
    /// </summary>
    /// <param name="title">string - Message title</param>
    /// <param name="message">string - MEssage body</param>
    public void LogAdminMessage(string title, string message)
    {
        AdminLogMessage adminmessage = new()
        {
            Id = 1,
            Title = title,
            Message = message
        };

        // Log the created name
        var lstAdminLogMessages = GetAdminLog();
        if (lstAdminLogMessages.Count > 0)
        {
            adminmessage.Id = lstAdminLogMessages.Max(x => x.Id) + 1;
        }

        lstAdminLogMessages.Add(adminmessage);
        var jsonAdminLogMessages = JsonSerializer.Serialize(lstAdminLogMessages);
        _fileSystemHelper.WriteFile(FileNames.AdminLogMessage, jsonAdminLogMessages);
        _cacheHelper.InvalidateCacheObject("AdminLogMessage");
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