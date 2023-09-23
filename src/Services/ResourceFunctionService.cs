using AzureNamingTool.Helpers;
using AzureNamingTool.Models;

namespace AzureNamingTool.Services;

public class ResourceFunctionService
{
    private readonly FileReader _fileReader;
    private readonly FileWriter _fileWriter;
    
    public ResourceFunctionService(
        FileReader reader, 
        FileWriter fileWriter)
    {
        _fileReader = reader;
        _fileWriter = fileWriter;
    }



    public ServiceResponse GetItems()
    {
        ServiceResponse serviceResponse = new();

        // Get list of items
        var items = _fileReader.GetList<ResourceFunction>();
        if (items != null)
        {
            serviceResponse.ResponseObject = items.OrderBy(x => x.SortOrder).ToList();
            serviceResponse.Success = true;
        }
        else
        {
            serviceResponse.ResponseObject = "Resource Functions not found!";
        }

        return serviceResponse;
    }

    public ServiceResponse GetItem(int id)
    {
        ServiceResponse serviceResponse = new();

        // Get list of items
        var items = _fileReader.GetList<ResourceFunction>();
        if (items != null)
        {
            var item = items.Find(x => x.Id == id);
            if (item != null)
            {
                serviceResponse.ResponseObject = item;
                serviceResponse.Success = true;
            }
            else
            {
                serviceResponse.ResponseObject = "Resource Function not found!";
            }
        }
        else
        {
            serviceResponse.ResponseObject = "Resource Functions not found!";
        }

        return serviceResponse;
    }

    public ServiceResponse PostItem(ResourceFunction item)
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
        var items = _fileReader.GetList<ResourceFunction>();
        if (items != null)
        {
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
                }

                // Reset the sort order of the list
                foreach (var thisitem in items.OrderBy(x => x.SortOrder).ToList())
                {
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
            _fileWriter.WriteList(items);
            serviceResponse.ResponseObject = "Resource Function added/updated!";
            serviceResponse.Success = true;
        }
        else
        {
            serviceResponse.ResponseObject = "Resource Functions not found!";
        }

        return serviceResponse;
    }

    public ServiceResponse DeleteItem(int id)
    {
        ServiceResponse serviceResponse = new();

        // Get list of items
        var items = _fileReader.GetList<ResourceFunction>();
        if (items != null)
        {
            // Get the specified item
            var item = items.Find(x => x.Id == id);
            if (item != null)
            {
                // Remove the item from the collection
                items.Remove(item);

                // Update all the sort order values to reflect the removal
                var position = 1;
                foreach (var thisitem in items.OrderBy(x => x.SortOrder).ToList())
                {
                    thisitem.SortOrder = position;
                    position += 1;
                }

                // Write items to file
                _fileWriter.WriteList(items);
                serviceResponse.Success = true;
            }
            else
            {
                serviceResponse.ResponseObject = "Resource Function not found!";
            }
        }
        else
        {
            serviceResponse.ResponseObject = "Resource Functions not found!";
        }

        return serviceResponse;
    }

    public ServiceResponse PostConfig(List<ResourceFunction> items)
    {
        ServiceResponse serviceResponse = new();

        // Get list of items
        var newitems = new List<ResourceFunction>();
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
            item.SortOrder = i;
            newitems.Add(item);
            i += 1;
        }

        // Write items to file
        _fileWriter.WriteList(newitems);
        serviceResponse.Success = true;

        return serviceResponse;
    }
}