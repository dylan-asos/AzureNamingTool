using System.Text.Json;
using AzureNamingTool.Helpers;
using AzureNamingTool.Models;

namespace AzureNamingTool.Services;

public class ResourceLocationService
{
    private readonly AdminLogService _adminLogService;
    private readonly CacheHelper _cacheHelper;
    private readonly ConfigurationHelper _configurationHelper;
    private readonly FileReader _fileReader;
    private readonly FileSystemHelper _fileSystemHelper;
    private readonly FileWriter _fileWriter;
    private readonly HttpContentDownloader _httpContentDownloader;

    public ResourceLocationService(
        CacheHelper cacheHelper,
        ConfigurationHelper configurationHelper,
        FileSystemHelper fileSystemHelper,
        AdminLogService adminLogService,
        FileReader fileReader, FileWriter fileWriter, HttpContentDownloader httpContentDownloader)
    {
        _cacheHelper = cacheHelper;
        _configurationHelper = configurationHelper;
        _fileSystemHelper = fileSystemHelper;
        _adminLogService = adminLogService;
        _fileReader = fileReader;
        _fileWriter = fileWriter;
        _httpContentDownloader = httpContentDownloader;
    }

    public async Task<ServiceResponse> GetItems(bool admin = true)
    {
        ServiceResponse serviceResponse = new();

        // Get list of items
        var items = await _fileReader.GetList<ResourceLocation>();

        if (!admin)
        {
            serviceResponse.ResponseObject = items.Where(x => x.Enabled).OrderBy(x => x.Name).ToList();
        }
        else
        {
            serviceResponse.ResponseObject = items.OrderBy(x => x.Name).ToList();
        }

        serviceResponse.Success = true;

        return serviceResponse;
    }

    public async Task<ServiceResponse> GetItem(int id)
    {
        ServiceResponse serviceResponse = new();

        // Get list of items
        var items = await _fileReader.GetList<ResourceLocation>();

        var item = items.Find(x => x.Id == id);
        if (item != null)
        {
            serviceResponse.ResponseObject = item;
            serviceResponse.Success = true;
        }
        else
        {
            serviceResponse.ResponseObject = "Resource Location not found!";
        }


        return serviceResponse;
    }

    public async Task<ServiceResponse> PostItem(ResourceLocation item)
    {
        ServiceResponse serviceResponse = new();

        // Make sure the new item short name only contains letters/numbers
        if (!ValidationHelper.CheckAlphanumeric(item.ShortName))
        {
            serviceResponse.Success = false;
            serviceResponse.ResponseObject = "Short name must be alphanumeric.";
            return serviceResponse;
        }

        // Force lowercase on the shortname
        item.ShortName = item.ShortName.ToLower();

        // Get list of items
        var items = await _fileReader.GetList<ResourceLocation>();

        // Set the new id
        if (item.Id == 0)
        {
            if (items.Count > 0)
            {
                item.Id = items.Max(t => t.Id) + 1;
            }
            else
            {
                item.Id = 1;
            }
        }

        // Determine new item id
        if (items.Count > 0)
        {
            // Check if the item already exists
            if (items.Exists(x => x.Id == item.Id))
            {
                // Remove the updated item from the list
                var existingitem = items.Find(x => x.Id == item.Id);
                if (existingitem != null)
                {
                    var index = items.IndexOf(existingitem);
                    items.RemoveAt(index);
                }
            }

            // Put the item at the end
            items.Add(item);
        }
        else
        {
            item.Id = 1;
            items.Add(item);
        }

        // Write items to file
        await _fileWriter.WriteList(items);
        serviceResponse.Success = true;

        return serviceResponse;
    }

    public async Task<ServiceResponse> DeleteItem(int id)
    {
        ServiceResponse serviceResponse = new();

        // Get list of items
        var items = await _fileReader.GetList<ResourceLocation>();

        // Get the specified item
        var item = items.Find(x => x.Id == id);
        if (item != null)
        {
            // Remove the item from the collection
            items.Remove(item);

            // Write items to file
            await _fileWriter.WriteList(items);
            serviceResponse.Success = true;
        }
        else
        {
            serviceResponse.ResponseObject = "Resource Location not found!";
        }

        return serviceResponse;
    }

    public async Task<ServiceResponse> PostConfig(List<ResourceLocation> items)
    {
        ServiceResponse serviceResponse = new();

        // Get list of items
        var newItems = new List<ResourceLocation>();
        var i = 1;

        // Determine new item id
        foreach (var item in items)
        {
            // Make sure the new item short name only contains letters/numbers
            if (!ValidationHelper.CheckAlphanumeric(item.ShortName))
            {
                serviceResponse.Success = false;
                serviceResponse.ResponseObject = "Short name must be alphanumeric.";
                return serviceResponse;
            }

            // Force lowercase on the shortname
            item.ShortName = item.ShortName.ToLower();

            item.Id = i;
            newItems.Add(item);
            i += 1;
        }

        // Write items to file
        await _fileWriter.WriteList(newItems);
        serviceResponse.Success = true;

        return serviceResponse;
    }

    public async Task<ServiceResponse> RefreshResourceLocations(bool shortNameReset = false)
    {
        var serviceResponse = await GetItems();
        var locations = (List<ResourceLocation>) serviceResponse.ResponseObject!;

        var url =
            "https://raw.githubusercontent.com/mspnp/AzureNamingTool/main/src/repository/resourcelocations.json";

        var refreshdata = await _httpContentDownloader.DownloadString(url);
        if (!string.IsNullOrEmpty(refreshdata))
        {
            var newlocations = new List<ResourceLocation>();
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true
            };

            newlocations = JsonSerializer.Deserialize<List<ResourceLocation>>(refreshdata, options);
            if (newlocations != null)
            {
                // Loop through the new items
                // Add any new resource location and update any existing locations
                foreach (var newlocation in newlocations)
                {
                    // Check if the existing locations contain the current location
                    var i = locations.FindIndex(x => x.Name == newlocation.Name);
                    if (i > -1)
                    {
                        // Update the Resource location Information
                        var oldlocation = locations[i];
                        newlocation.Enabled = oldlocation.Enabled;

                        if (!shortNameReset || string.IsNullOrEmpty(oldlocation.ShortName))
                        {
                            newlocation.ShortName = oldlocation.ShortName;
                        }

                        // Remove the old location
                        locations.RemoveAt(i);
                        // Add the new location
                        locations.Add(newlocation);
                    }
                    else
                    {
                        // Add a new resource location
                        locations.Add(newlocation);
                    }
                }

                // Update the settings file
                serviceResponse = await PostConfig(locations);

                // Update the repository file
                _fileSystemHelper.WriteFile(FileNames.ResourceLocation, refreshdata, "repository/");

                // Clear cached data
                _cacheHelper.InvalidateCacheObject("ResourceLocation");

                // Update the current configuration file version data information
                await _configurationHelper.UpdateConfigurationFileVersion("resourcelocations");
            }
            else
            {
                serviceResponse.ResponseObject = "Resource Locations not found!";
            }
        }
        else
        {
            serviceResponse.ResponseObject = "Refresh Resource Locations not found!";
        }
        
        return serviceResponse;
    }
}