using AzureNamingTool.Helpers;
using AzureNamingTool.Models;

namespace AzureNamingTool.Services;

public class ResourceDelimiterService
{
    private readonly FileReader _fileReader;
    private readonly FileWriter _fileWriter;

    public ResourceDelimiterService(
        FileReader reader,
        FileWriter fileWriter)
    {
        _fileReader = reader;
        _fileWriter = fileWriter;
    }

    public async Task<ServiceResponse> GetItems(bool admin)
    {
        ServiceResponse serviceResponse = new();

        // Get list of items
        var items = await _fileReader.GetList<ResourceDelimiter>();

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
        var items = await _fileReader.GetList<ResourceDelimiter>();

        var item = items.Find(x => x.Id == id);
        if (item != null)
        {
            serviceResponse.ResponseObject = item;
            serviceResponse.Success = true;
        }
        else
        {
            serviceResponse.ResponseObject = "Resource Delimiter not found!";
        }

        return serviceResponse;
    }

    public async Task<ServiceResponse> GetCurrentItem()
    {
        ServiceResponse serviceResponse = new();

        // Get list of items
        var items = await _fileReader.GetList<ResourceDelimiter>();

        serviceResponse.ResponseObject =
            items.OrderBy(y => y.SortOrder).ThenByDescending(y => y.Enabled).ToList()[0];
        serviceResponse.Success = true;

        return serviceResponse;
    }

    public async Task<ServiceResponse> PostItem(ResourceDelimiter item)
    {
        ServiceResponse serviceResponse = new();

        // Get list of items
        var items = await _fileReader.GetList<ResourceDelimiter>();

        // Set the new id
        if (item.Id == 0)
        {
            item.Id = items.Count + 1;
        }

        item.Enabled = true;
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
                var existingitem = items.Find(x => x.Id == item.Id);
                if (existingitem != null)
                {
                    var index = items.IndexOf(existingitem);
                    items.RemoveAt(index);
                }

                // Reset the sort order of the list
                foreach (var thisitem in items.OrderBy(x => x.SortOrder).ToList())
                {
                    if (item.Enabled && thisitem.Id != item.Id)
                    {
                        thisitem.Enabled = false;
                    }

                    thisitem.SortOrder = position;
                    position += 1;
                }

                // Check for the new sort order
                if (items.Exists(x => x.SortOrder == item.SortOrder))
                {
                    // Remove the updated item from the list
                    items.Insert(items.IndexOf(items.FirstOrDefault(x => x.SortOrder == item.SortOrder)!), item);
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
            foreach (var thisitem in items.OrderBy(x => x.SortOrder).ToList())
            {
                thisitem.SortOrder = position;
                position += 1;
            }

            // Write items to file
            await _fileWriter.WriteList(items);
            serviceResponse.ResponseObject = "Resource Delimiter added/updated!";
            serviceResponse.Success = true;
        }

        return serviceResponse;
    }

    public async Task<ServiceResponse> PostConfig(List<ResourceDelimiter> items)
    {
        ServiceResponse serviceResponse = new();

        var delimiters = new string[4, 2] {{"dash", "-"}, {"underscore", "_"}, {"period", "."}, {"none", ""}};
        var newitems = new List<ResourceDelimiter>();

        // Examine the current items
        foreach (var item in items)
        {
            // Check if the item is valid
            for (var j = 0; j <= delimiters.GetUpperBound(0); j++)
            {
                if (item.Name == delimiters[j, 0] && item.Delimiter == delimiters[j, 1])
                {
                    // Add the item to the update list
                    newitems.Add(item);
                    break;
                }
            }
        }

        // Make sure all the delimiters are present
        for (var k = 0; k <= delimiters.GetUpperBound(0); k++)
        {
            if (!newitems.Exists(x => x.Name == delimiters[k, 0] && x.Delimiter == delimiters[k, 1]))
            {
                // Create a delimiter object 
                ResourceDelimiter newitem = new()
                {
                    Name = delimiters[k, 0],
                    Delimiter = delimiters[k, 1],
                    Enabled = false
                };
                newitems.Add(newitem);
            }
        }

        // Determine new item ids/order
        var i = 1;
        var sortedItems = newitems
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Enabled);

        foreach (var item in sortedItems)
        {
            item.Id = i;
            item.SortOrder = i;
            i += 1;
        }

        // Write items to file
        await _fileWriter.WriteList(newitems);
        serviceResponse.Success = true;

        return serviceResponse;
    }
}