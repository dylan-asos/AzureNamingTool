using AzureNamingTool.Helpers;
using AzureNamingTool.Models;

namespace AzureNamingTool.Services;

public class GeneratedNamesService
{
    private readonly CacheHelper _cacheHelper;
    private readonly FileReader _fileReader;
    private readonly FileWriter _fileWriter;

    public GeneratedNamesService(
        CacheHelper cacheHelper, 
        FileReader fileReader,
        FileWriter fileWriter)
    {
        _cacheHelper = cacheHelper;
        _fileReader = fileReader;
        _fileWriter = fileWriter;
    }

    /// <summary>
    ///     This function gets the generated names log.
    /// </summary>
    /// <returns>List of GeneratedNames - List of generated names</returns>
    public ServiceResponse GetItems()
    {
        ServiceResponse serviceResponse = new();
        
        var items = _fileReader.GetList<GeneratedName>();
        if (items!= null)
        {
            serviceResponse.ResponseObject = items.OrderByDescending(x => x.CreatedOn).ToList();
            serviceResponse.Success = true;
        }

        return serviceResponse;
    }

    public ServiceResponse GetItem(int id)
    {
        ServiceResponse serviceResponse = new();

        // Get list of items
        var items = _fileReader.GetList<GeneratedName>();
        if (items!= null)
        {
            var item = items.Find(x => x.Id == id);
            if (item!= null)
            {
                serviceResponse.ResponseObject = item;
                serviceResponse.Success = true;
            }
            else
            {
                serviceResponse.ResponseObject = "Generated Name not found!";
            }
        }
        else
        {
            serviceResponse.ResponseObject = "Generated Names not found!";
        }

        return serviceResponse;
    }

    /// <summary>
    ///     This function logs the generated name.
    /// </summary>
    /// <param name="generatedName">GeneratedName - Generated name and components.</param>
    public ServiceResponse PostItem(GeneratedName generatedName)
    {
        ServiceResponse serviceResponse = new();

        // Get the previously generated names
        var items = _fileReader.GetList<GeneratedName>();
        if (items!= null)
        {
            if (items.Count > 0)
            {
                generatedName.Id = items.Max(x => x.Id) + 1;
            }
            else
            {
                generatedName.Id = 1;
            }

            items.Add(generatedName);

            // Write items to file
            _fileWriter.WriteList(items);

            _cacheHelper.InvalidateCacheObject(FileNames.GeneratedName);

            serviceResponse.Success = true;
        }

        return serviceResponse;
    }

    public ServiceResponse DeleteItem(int id)
    {
        ServiceResponse serviceResponse = new();

        // Get list of items
        var items = _fileReader.GetList<GeneratedName>();
        if (items!= null)
        {
            // Get the specified item
            var item = items.Find(x => x.Id == id);
            if (item!= null)
            {
                // Remove the item from the collection
                items.Remove(item);

                // Write items to file
                _fileWriter.WriteList(items);
                serviceResponse.Success = true;
            }
            else
            {
                serviceResponse.ResponseObject = "Generated Name not found!";
            }
        }
        else
        {
            serviceResponse.ResponseObject = "Generated Name not found!";
        }

        return serviceResponse;
    }

    /// <summary>
    ///     This function clears the generated names log.
    /// </summary>
    /// <returns>void</returns>
    public ServiceResponse DeleteAllItems()
    {
        ServiceResponse serviceResponse = new();

        List<GeneratedName> items = new();
        _fileWriter.WriteList(items);
        serviceResponse.Success = true;

        return serviceResponse;
    }

    public ServiceResponse PostConfig(List<GeneratedName> items)
    {
        ServiceResponse serviceResponse = new();

        // Get list of items
        var newitems = new List<GeneratedName>();
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