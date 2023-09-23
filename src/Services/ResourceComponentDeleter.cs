using AzureNamingTool.Helpers;
using AzureNamingTool.Models;

namespace AzureNamingTool.Services;

public class ResourceComponentDeleter
{
    private readonly FileReader _fileReader;
    private readonly FileWriter _fileWriter;
    private readonly ResourceTypeService _resourceTypeService;
    private readonly GeneralHelper _generalHelper;

    public ResourceComponentDeleter(FileReader fileReader, FileWriter fileWriter, ResourceTypeService resourceTypeService, GeneralHelper generalHelper)
    {
        _fileReader = fileReader;
        _fileWriter = fileWriter;
        _resourceTypeService = resourceTypeService;
        _generalHelper = generalHelper;
    }

    public ServiceResponse DeleteItem(int id)
    {
        ServiceResponse serviceResponse = new();

        // Get list of items
        var items = _fileReader.GetList<ResourceComponent>();
        if (items != null)
        {
            // Get the specified item
            var item = items.Find(x => x.Id == id && x.IsCustom);
            if (item != null)
            {
                // Delete any resource type settings for the component
                List<string> currentvalues = new();
                serviceResponse = _resourceTypeService.GetItems();
                if (serviceResponse.ResponseObject != null)
                {
                    var resourceTypes = (List<ResourceType>) serviceResponse.ResponseObject!;
                    if (resourceTypes != null)
                    {
                        foreach (var currenttype in resourceTypes)
                        {
                            currentvalues = new List<string>(currenttype.Optional.Split(','));
                            if (currentvalues.Contains(_generalHelper.NormalizeName(item.Name, false)))
                            {
                                currentvalues.Remove(_generalHelper.NormalizeName(item.Name, false));
                                currenttype.Optional = string.Join(",", currentvalues.ToArray());
                            }

                            currentvalues = new List<string>(currenttype.Exclude.Split(','));
                            if (currentvalues.Contains(_generalHelper.NormalizeName(item.Name, false)))
                            {
                                currentvalues.Remove(_generalHelper.NormalizeName(item.Name, false));
                                currenttype.Exclude = string.Join(",", currentvalues.ToArray());
                            }

                            _resourceTypeService.PostItem(currenttype);
                        }

                        // Delete any custom components for this resource component
                        var components = _fileReader.GetList<CustomComponent>();
                        if (components != null)
                        {
                            components.RemoveAll(x =>
                                x.ParentComponent == _generalHelper.NormalizeName(item.Name, true));
                            _fileWriter.WriteList(components);

                            // Remove the item from the collection
                            items.Remove(item);

                            // Update all the sort order values to reflect the removal
                            var position = 1;
                            foreach (var thisitem in items.OrderBy(x => x.SortOrder).ToList())
                            {
                                thisitem.SortOrder = position;
                                thisitem.Id = position;
                                position += 1;
                            }

                            // Write items to file
                            _fileWriter.WriteList(items);
                            serviceResponse.Success = true;
                        }
                        else
                        {
                            serviceResponse.ResponseObject = "Resource Components not found!";
                        }
                    }
                    else
                    {
                        serviceResponse.ResponseObject = "Resource Types not found!";
                    }
                }
            }
            else
            {
                serviceResponse.ResponseObject = "Resource Component not found!";
            }
        }
        else
        {
            serviceResponse.ResponseObject = "Resource Components not found!";
        }

        return serviceResponse;
    }
}