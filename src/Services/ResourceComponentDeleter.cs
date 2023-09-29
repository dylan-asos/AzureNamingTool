using AzureNamingTool.Helpers;
using AzureNamingTool.Models;

namespace AzureNamingTool.Services;

public class ResourceComponentDeleter
{
    private readonly FileReader _fileReader;
    private readonly FileWriter _fileWriter;
    private readonly GeneralHelper _generalHelper;
    private readonly ResourceTypeService _resourceTypeService;

    public ResourceComponentDeleter(FileReader fileReader, FileWriter fileWriter,
        ResourceTypeService resourceTypeService, GeneralHelper generalHelper)
    {
        _fileReader = fileReader;
        _fileWriter = fileWriter;
        _resourceTypeService = resourceTypeService;
        _generalHelper = generalHelper;
    }

    public async Task<ServiceResponse> DeleteItem(int id)
    {
        ServiceResponse serviceResponse = new();

        var items = await _fileReader.GetList<ResourceComponent>();

        // Get the specified item
        var item = items.Find(x => x.Id == id && x.IsCustom);
        if (item == null)
        {
            serviceResponse.ResponseObject = "Resource Component not found!";
            return serviceResponse;
        }

        // Delete any resource type settings for the component
        serviceResponse = await _resourceTypeService.GetItems();
        if (serviceResponse.ResponseObject == null)
            return serviceResponse;

        var resourceTypes = (List<ResourceType>) serviceResponse.ResponseObject!;

        foreach (var currenttype in resourceTypes)
        {
            var currentvalues = new List<string>(currenttype.Optional.Split(','));
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

            await _resourceTypeService.PostItem(currenttype);
        }

        // Delete any custom components for this resource component
        var components = await _fileReader.GetList<CustomComponent>();

        components.RemoveAll(x =>
            x.ParentComponent == _generalHelper.NormalizeName(item.Name, true));

        await _fileWriter.WriteList(components);

        // Remove the item from the collection
        items.Remove(item);

        // Update all the sort order values to reflect the removal
        var position = 1;
        foreach (var thisItem in items.OrderBy(x => x.SortOrder).ToList())
        {
            thisItem.SortOrder = position;
            thisItem.Id = position;
            position += 1;
        }

        // Write items to file
        await _fileWriter.WriteList(items);
        serviceResponse.Success = true;

        return serviceResponse;
    }
}