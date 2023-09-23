using AzureNamingTool.Helpers;
using AzureNamingTool.Models;

namespace AzureNamingTool.Services;

public class AdminUserService
{
    private readonly FileReader _fileReader;
    private readonly FileWriter _fileWriter;
    
    public AdminUserService(
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
        var items = _fileReader.GetList<AdminUser>();
        if (items != null)
        {
            serviceResponse.ResponseObject = items.OrderBy(x => x.Name).ToList();
            serviceResponse.Success = true;
        }

        return serviceResponse;
    }

    public ServiceResponse GetItem(string name)
    {
        ServiceResponse serviceResponse = new();

        // Get list of items
        var items = _fileReader.GetList<AdminUser>();
        if (items != null)
        {
            var item = items.Find(x => x.Name == name);
            serviceResponse.ResponseObject = item;
            serviceResponse.Success = true;
        }

        return serviceResponse;
    }

    public ServiceResponse PostItem(AdminUser item)
    {
        ServiceResponse serviceResponse = new();

        // Get list of items
        var items = _fileReader.GetList<AdminUser>();
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

            items = items.OrderBy(x => x.Name).ToList();

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

                // Check for the new sort order
                if (items.Exists(x => x.Id == item.Id))
                {
                    // Remove the updated item from the list
                    items.Insert(items.IndexOf(items.FirstOrDefault(x => x.Id == item.Id)!), item);
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
                items.Add(item);
            }

            // Write items to file
            _fileWriter.WriteList(items);
            serviceResponse.ResponseObject = "Item added!";
            serviceResponse.Success = true;
        }


        return serviceResponse;
    }

    public ServiceResponse DeleteItem(int id)
    {
        ServiceResponse serviceResponse = new();

        // Get list of items
        var items =  _fileReader.GetList<AdminUser>();
        if (items != null)
        {
            // Get the specified item
            var item = items.Find(x => x.Id == id);
            if (item != null)
            {
                // Remove the item from the collection
                items.Remove(item);

                // Write items to file
                _fileWriter.WriteList(items);
                serviceResponse.Success = true;
            }
        }

        return serviceResponse;
    }

    public ServiceResponse PostConfig(List<AdminUser> items)
    {
        ServiceResponse serviceResponse = new();

        // Get list of items
        var newitems = new List<AdminUser>();
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