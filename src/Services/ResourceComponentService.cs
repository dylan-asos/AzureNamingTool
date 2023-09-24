using System.Text.Json;
using AzureNamingTool.Helpers;
using AzureNamingTool.Models;

namespace AzureNamingTool.Services;

public class ResourceComponentService
{
    private readonly FileReader _fileReader;
    private readonly FileSystemHelper _fileSystemHelper;
    private readonly FileWriter _fileWriter;

    public ResourceComponentService(
        FileReader fileReader,
        FileWriter fileWriter,
        FileSystemHelper fileSystemHelper)
    {
        _fileReader = fileReader;
        _fileWriter = fileWriter;
        _fileSystemHelper = fileSystemHelper;
    }

    public async Task<ServiceResponse> GetItems(bool admin)
    {
        ServiceResponse serviceResponse = new();

        var items = await _fileReader.GetList<ResourceComponent>();

        if (!admin)
        {
            serviceResponse.ResponseObject = items.Where(x => x.Enabled).OrderBy(y => y.SortOrder).ToList();
        }
        else
        {
            serviceResponse.ResponseObject =
                items.OrderBy(y => y.SortOrder).ThenByDescending(y => y.Enabled).ToList();
        }

        serviceResponse.Success = true;

        return serviceResponse;
    }

    public async Task<ServiceResponse> GetItem(int id)
    {
        ServiceResponse serviceResponse = new();

        // Get list of items
        var items = await _fileReader.GetList<ResourceComponent>();

        var item = items.Find(x => x.Id == id);
        if (item != null)
        {
            serviceResponse.ResponseObject = item;
            serviceResponse.Success = true;
        }
        else
        {
            serviceResponse.ResponseObject = "Resource Component not found!";
        }


        return serviceResponse;
    }

    public async Task<ServiceResponse> PostItem(ResourceComponent item)
    {
        ServiceResponse serviceResponse = new();

        // Get list of items
        var items = await _fileReader.GetList<ResourceComponent>();

        // Set the new id
        if (item.Id == 0)
        {
            item.Id = items.Count + 1;
        }

        var position = 1;
        items = items.OrderBy(x => x.SortOrder).ToList();

        if (item.SortOrder == 0)
        {
            item.SortOrder = items.Count + 1;
        }

        // Determine new item id
        if (items.Count > 0)
        {
            // Check if the item already exists
            if (items.Exists(x => x.Id == item.Id))
            {
                // Remove the updated item from the list
                var existingItem = items.Find(x => x.Id == item.Id);
                if (existingItem != null)
                {
                    var index = items.IndexOf(existingItem);
                    items.RemoveAt(index);
                }
            }

            // Reset the sort order of the list
            foreach (var thisItem in items
                         .OrderBy(x => x.SortOrder)
                         .ThenByDescending(x => x.Enabled)
                         .ToList())
            {
                thisItem.SortOrder = position;
                position += 1;
            }

            // Check for the new sort order
            if (items.Exists(x => x.SortOrder == item.SortOrder))
            {
                // Insert the new item
                items.Insert(items.IndexOf(items.FirstOrDefault(x => x.SortOrder == item.SortOrder)!),
                    item);
            }
            else
            {
                // Put the item at the end
                items.Add(item);
            }
        }
        else
        {
            item.Id = 1;
            item.SortOrder = 1;
            items.Add(item);
        }

        position = 1;
        foreach (var thisitem in items.OrderBy(x => x.SortOrder).ThenByDescending(x => x.Enabled).ToList())
        {
            thisitem.SortOrder = position;
            thisitem.Id = position;
            position += 1;
        }

        // Write items to file
        _fileWriter.WriteList(items);
        serviceResponse.Success = true;

        return serviceResponse;
    }

    public ServiceResponse PostConfig(List<ResourceComponent> items)
    {
        ServiceResponse serviceResponse = new();

        var componentNames = new string[8]
        {
            "ResourceEnvironment", "ResourceInstance", "ResourceLocation", "ResourceOrg", "ResourceProjAppSvc",
            "ResourceType", "ResourceUnitDept", "ResourceFunction"
        };
        var newitems = new List<ResourceComponent>();

        // Examine the current items
        foreach (var item in items)
        {
            // Check if the item is valid
            if (!componentNames.Contains(item.Name))
            {
                item.IsCustom = true;
            }

            // Add the item to the update list
            newitems.Add(item);
        }

        // Make sure all the component names are present
        foreach (var name in componentNames)
        {
            if (newitems.Exists(x => x.Name == name))
                continue;

            // Create a component object 
            ResourceComponent newItem = new()
            {
                Name = name,
                Enabled = false
            };
            newitems.Add(newItem);
        }

        // Determine new item ids
        var i = 1;

        foreach (var item in newitems.OrderByDescending(x => x.Enabled).ThenBy(x => x.SortOrder))
        {
            item.Id = i;
            item.SortOrder = i;
            i += 1;
        }

        // Write items to file
        _fileWriter.WriteList(newitems);
        serviceResponse.Success = true;

        return serviceResponse;
    }

    /// <summary>
    ///     This function is used to sync default configuration data with the user's local version
    /// </summary>
    /// <param name="type">string - Type of configuration data to sync</param>
    public async Task SyncConfigurationData()
    {
        var update = false;

        var serviceResponse = await GetItems(true);
        if (!serviceResponse.Success)
            return;

        List<ResourceComponent> currentComponents = serviceResponse.ResponseObject!;
        // Get the default component data
        List<ResourceComponent> defaultComponents = new();
        var data = _fileSystemHelper.ReadFile(FileNames.ResourceComponent, "repository/");
        if (string.IsNullOrEmpty(data))
            return;

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };

        if (!string.IsNullOrEmpty(data))
        {
            defaultComponents =
                JsonSerializer.Deserialize<List<ResourceComponent>>(data, options)!;
        }

        // Loop over the existing components to verify the data is complete
        foreach (var currentComponent in currentComponents)
        {
            // Create a new component for any updates
            var newComponent = currentComponent;

            // Get the matching default component for the current component
            var defaultComponent = defaultComponents.Find(x => x.Name == currentComponent.Name);

            // Check the data to see if it's been configured
            if (string.IsNullOrEmpty(currentComponent.MinLength))
            {
                newComponent.MinLength = defaultComponent != null
                    ? defaultComponent.MinLength
                    : "1";

                update = true;
            }

            // Check the data to see if it's been configured
            if (string.IsNullOrEmpty(currentComponent.MaxLength))
            {
                newComponent.MaxLength = defaultComponent != null
                    ? defaultComponent.MaxLength
                    : "10";

                update = true;
            }

            if (update)
            {
                await PostItem(newComponent);
            }
        }
    }
}