using System.Text.Json;
using AzureNamingTool.Helpers;
using AzureNamingTool.Models;

namespace AzureNamingTool.Services;

public class ResourceTypeService
{
    private readonly AdminLogService _adminLogService;
    private readonly CacheHelper _cacheHelper;
    private readonly ConfigurationHelper _configurationHelper;
    private readonly FileReader _fileReader;
    private readonly FileSystemHelper _fileSystemHelper;
    private readonly FileWriter _fileWriter;
    private readonly HttpContentDownloader _httpContentDownloader;
    private readonly ResourceDelimiterService _resourceDelimiterService;
    private readonly ValidationHelper _validationHelper;

    public ResourceTypeService(
        CacheHelper cacheHelper,
        FileSystemHelper fileSystemHelper,
        ValidationHelper validationHelper,
        ConfigurationHelper configurationHelper,
        AdminLogService adminLogService,
        ResourceDelimiterService resourceDelimiterService,
        GeneralHelper generalHelper,
        ResourceComponentService resourceComponentService,
        FileReader fileReader,
        FileWriter fileWriter, HttpContentDownloader httpContentDownloader)
    {
        _cacheHelper = cacheHelper;
        _fileSystemHelper = fileSystemHelper;
        _validationHelper = validationHelper;
        _configurationHelper = configurationHelper;
        _adminLogService = adminLogService;
        _resourceDelimiterService = resourceDelimiterService;
        _fileReader = fileReader;
        _fileWriter = fileWriter;
        _httpContentDownloader = httpContentDownloader;
    }

    public ServiceResponse GetItems(bool admin = true)
    {
        ServiceResponse serviceResponse = new();

        // Get list of items
        var items = _fileReader.GetList<ResourceType>();
        if (items != null)
        {
            if (!admin)
            {
                serviceResponse.ResponseObject = items.Where(x => x.Enabled).OrderBy(x => x.Resource).ToList();
            }
            else
            {
                serviceResponse.ResponseObject = items.OrderBy(x => x.Resource).ToList();
            }

            serviceResponse.Success = true;
        }
        else
        {
            serviceResponse.ResponseObject = "Resource Types not found!";
        }


        return serviceResponse;
    }

    public ServiceResponse GetItem(int id)
    {
        ServiceResponse serviceResponse = new();

        // Get list of items
        var items = _fileReader.GetList<ResourceType>();
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
                serviceResponse.ResponseObject = "Resource Type not found!";
            }
        }
        else
        {
            serviceResponse.ResponseObject = "Resource Types not found!";
        }

        return serviceResponse;
    }

    public ServiceResponse PostItem(ResourceType item)
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
        var items = _fileReader.GetList<ResourceType>();
        if (items!= null)
        {
            // Set the new id
            if (item.Id == 0)
            {
                item.Id = items.Count + 1;
            }

            // Determine new item id
            if (items.Count > 0)
            {
                // Check if the item already exists
                if (items.Exists(x => x.Id == item.Id))
                {
                    // Remove the updated item from the list
                    var existingitem = items.Find(x => x.Id == item.Id);
                    if (existingitem!= null)
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
            _fileWriter.WriteList(items.OrderBy(x => x.Id).ToList());
            serviceResponse.ResponseObject = "Resource Type added/updated!";
            serviceResponse.Success = true;
        }
        else
        {
            serviceResponse.ResponseObject = "Resource Types not found!";
        }


        return serviceResponse;
    }

    public ServiceResponse DeleteItem(int id)
    {
        ServiceResponse serviceResponse = new();

        // Get list of items
        var items = _fileReader.GetList<ResourceType>();
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
                serviceResponse.ResponseObject = "Resource Type not found!";
            }
        }
        else
        {
            serviceResponse.ResponseObject = "Resource Types not found!";
        }

        return serviceResponse;
    }

    public ServiceResponse PostConfig(List<ResourceType> items)
    {
        ServiceResponse serviceResponse = new();

        // Get list of items
        var newitems = new List<ResourceType>();
        var i = 1;

        // Determine new item id
        foreach (var item in items)
        {
            // Force lowercase on the shortname
            item.ShortName = item.ShortName.ToLower();
            item.Id = i;
            newitems.Add(item);
            i += 1;
        }

        // Write items to file
        _fileWriter.WriteList(newitems);
        serviceResponse.Success = true;

        return serviceResponse;
    }

    public List<string> GetTypeCategories(List<ResourceType> types)
    {
        ServiceResponse serviceResponse = new();
        List<string> categories = new();

        foreach (var type in types)
        {
            var category = type.Resource;
            if (!string.IsNullOrEmpty(category))
            {
                if (category.Contains('/'))
                {
                    category = category[..category.IndexOf("/")];
                }

                if (!categories.Contains(category))
                {
                    categories.Add(category);
                }
            }
            else
            {
                serviceResponse.ResponseObject = "Resource Type Categories not found!";
            }
        }

        return categories;
    }

    public static List<ResourceType> GetFilteredResourceTypes(List<ResourceType> types, string filter)
    {
        List<ResourceType> currenttypes = new();
        // Filter out resource types that should have name generation
        if (!string.IsNullOrEmpty(filter))
        {
            currenttypes = types.Where(x =>
                x.Resource.ToLower().StartsWith(filter.ToLower() + "/") && x.Property.ToLower() != "display name" &&
                !string.IsNullOrEmpty(x.ShortName)).ToList();
        }
        else
        {
            currenttypes = types;
        }

        return currenttypes;
    }

    public async Task<ServiceResponse> RefreshResourceTypes(bool shortNameReset = false)
    {
        var serviceResponse = GetItems();
        if (serviceResponse.Success)
        {
            var types = (List<ResourceType>) serviceResponse.ResponseObject!;
            if (types!= null)
            {
                var url =
                    "https://raw.githubusercontent.com/mspnp/AzureNamingTool/main/src/repository/resourcetypes.json";
                var refreshdata = await _httpContentDownloader.DownloadString(url);
                if (!string.IsNullOrEmpty(refreshdata))
                {
                    var newtypes = new List<ResourceType>();
                    var options = new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                        PropertyNameCaseInsensitive = true
                    };

                    newtypes = JsonSerializer.Deserialize<List<ResourceType>>(refreshdata, options);
                    if (newtypes!= null)
                    {
                        // Loop through the new items
                        // Add any new resource type and update any existing types
                        foreach (var newtype in newtypes)
                        {
                            // Check if the existing types contain the current type
                            var i = types.FindIndex(x => x.Resource == newtype.Resource);
                            if (i > -1)
                            {
                                // Update the Resource Type Information
                                var oldtype = types[i];
                                newtype.Exclude = oldtype.Exclude;
                                newtype.Optional = oldtype.Optional;
                                newtype.Enabled = oldtype.Enabled;
                                if (!shortNameReset || string.IsNullOrEmpty(oldtype.ShortName))
                                {
                                    newtype.ShortName = oldtype.ShortName;
                                }

                                // Remove the old type
                                types.RemoveAt(i);
                                // Add the new type
                                types.Add(newtype);
                            }
                            else
                            {
                                // Add a new resource type
                                types.Add(newtype);
                            }
                        }

                        // Update the settings file
                        serviceResponse = PostConfig(types);

                        // Update the repository file
                        _fileSystemHelper.WriteFile(FileNames.ResourceType, refreshdata, "repository/");

                        // Clear cached data
                        _cacheHelper.InvalidateCacheObject("ResourceType");

                        // Update the current configuration file version data information
                        await _configurationHelper.UpdateConfigurationFileVersion("resourcetypes");
                    }
                    else
                    {
                        serviceResponse.ResponseObject = "Resource Types not found!";
                    }
                }
                else
                {
                    serviceResponse.ResponseObject = "Refresh Resource Types not found!";
                    _adminLogService.PostItem(new AdminLogMessage
                    {
                        Title = "ERROR",
                        Message = "There was a problem refreshing the resource types configuration."
                    });
                }
            }
            else
            {
                serviceResponse.ResponseObject = "Resource Types not found!";
            }
        }
        else
        {
            serviceResponse.ResponseObject = "Resource Types not found!";
        }

        return serviceResponse;
    }
    
    public ServiceResponse ValidateResourceTypeName(ValidateNameRequest validateNameRequest)
    {
        ValidateNameResponse validateNameResponse = new();
        ResourceDelimiter? resourceDelimiter = new();

        var serviceResponse = _resourceDelimiterService.GetCurrentItem();
        if (serviceResponse.Success)
        {
            if (serviceResponse.ResponseObject != null)
            {
                resourceDelimiter = serviceResponse.ResponseObject as ResourceDelimiter;
            }
        }
        else
        {
            serviceResponse.ResponseObject = "Delimiter value could not be set.";
            serviceResponse.Success = false;
            return serviceResponse;
        }

        // Get the specifed resource type
        serviceResponse = GetItems();
        if (serviceResponse.Success)
        {
            if (serviceResponse.ResponseObject != null)
            {
                // Get the resource types
                var resourceTypes = (List<ResourceType>) serviceResponse.ResponseObject!;
                if (resourceTypes!= null)
                {
                    // Get the specified resoure type
                    var resourceType =
                        resourceTypes.FirstOrDefault(x => x.ShortName == validateNameRequest.ResourceType)!;
                    if (resourceType!= null)
                    {
                        // Create a validate name request
                        validateNameResponse = _validationHelper.ValidateGeneratedName(resourceType,
                            validateNameRequest.Name!, resourceDelimiter!.Delimiter);
                    }
                    else
                    {
                        validateNameResponse.Message = "Resoruce Type is invalid!";
                        validateNameResponse.Valid = false;
                    }
                }
                else
                {
                    validateNameResponse.Message = "Resoruce Type is invalid!";
                    validateNameResponse.Valid = false;
                }
            }
            else
            {
                validateNameResponse.Message = "Resoruce Type is invalid!";
                validateNameResponse.Valid = false;
            }
        }

        serviceResponse.ResponseObject = validateNameResponse;
        serviceResponse.Success = true;

        return serviceResponse;
    }
}