using AzureNamingTool.Helpers;
using AzureNamingTool.Models;
using AzureNamingTool.Pages;

namespace AzureNamingTool.Services;

public class AdminLogService
{
    private readonly FileReader _fileReader;
    private readonly FileWriter _fileWriter;
    private readonly ILogger<AdminLogService> _logger;

    public AdminLogService(
        FileReader reader, 
        FileWriter fileWriter,
        ILogger<AdminLogService> logger)
    {
        _fileReader = reader;
        _fileWriter = fileWriter;
        _logger = logger;
    }
    
    /// <summary>
    ///     This function returns the Admin log.
    /// </summary>
    /// <returns>List of AdminLogMessages - List of Adming Log messages.</returns>
    public ServiceResponse GetItems()
    {
        ServiceResponse serviceResponse = new();

        // Get list of items
        var items = _fileReader.GetList<AdminLogMessage>();
        if (items == null) 
            return serviceResponse;
        
        serviceResponse.ResponseObject = items.OrderByDescending(x => x.CreatedOn).ToList();
        serviceResponse.Success = true;

        return serviceResponse;
    }

    /// <summary>
    ///     This function logs the Admin message.
    /// </summary>
    public void PostItem(AdminLogMessage adminLogMessage)
    {
        // Log the created name
        var items = _fileReader.GetList<AdminLogMessage>();

        if (items == null)
            return;
        
        if (items.Count > 0)
        {
            adminLogMessage.Id = items.Max(x => x.Id) + 1;
        }
        
        _logger.LogInformation(adminLogMessage.Message);

        items.Add(adminLogMessage);
        _fileWriter.WriteList(items);
    }

    /// <summary>
    ///     This function clears the Admin log.
    /// </summary>
    /// <returns>void</returns>
    public ServiceResponse DeleteAllItems()
    {
        ServiceResponse serviceResponse = new();

        List<AdminLogMessage> lstAdminLogMessages = new();
        _fileWriter.WriteList(lstAdminLogMessages);
        serviceResponse.Success = true;

        return serviceResponse;
    }

    public ServiceResponse PostConfig(List<AdminLogMessage> items)
    {
        ServiceResponse serviceResponse = new();

        // Get list of items
        var newitems = new List<AdminLogMessage>();
        var i = 1;

        // Determine new item id
        foreach (var item in items)
        {
            item.Id = i;
            newitems.Add(item);
            i += 1;
        }

        // Write items to file
        _fileWriter.WriteList(newitems);
        serviceResponse.Success = true;

        return serviceResponse;
    }
}