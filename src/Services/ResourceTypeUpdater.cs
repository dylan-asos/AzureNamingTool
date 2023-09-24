using AzureNamingTool.Helpers;
using AzureNamingTool.Models;

namespace AzureNamingTool.Services;

public class ResourceTypeUpdater
{
    private readonly GeneralHelper _generalHelper;
    private readonly ResourceComponentService _resourceComponentService;
    private readonly ResourceTypeService _resourceTypeService;

    public ResourceTypeUpdater(
        ResourceComponentService resourceComponentService,
        GeneralHelper generalHelper,
        ResourceTypeService resourceTypeService)
    {
        _resourceComponentService = resourceComponentService;
        _generalHelper = generalHelper;
        _resourceTypeService = resourceTypeService;
    }

    public async Task<ServiceResponse> UpdateTypeComponents(string operation, int componentId)
    {
        var serviceResponse = await _resourceComponentService.GetItem(componentId);

        if (!serviceResponse.ResponseObject != null)
            return serviceResponse;

        var resourceComponent = (ResourceComponent) serviceResponse.ResponseObject!;

        var component = _generalHelper.NormalizeName(resourceComponent.Name, false);
        serviceResponse = await _resourceTypeService.GetItems();
        
        if (serviceResponse.Success)
        {
            if (serviceResponse.ResponseObject == null)
                return serviceResponse;

            var resourceTypes = (List<ResourceType>) serviceResponse.ResponseObject!;
            // Update all the resource type component settings
            foreach (var currentType in resourceTypes)
            {
                List<string> currentValues;
                switch (operation)
                {
                    case "optional-add":
                        currentValues = new List<string>(currentType.Optional.Split(','));
                        if (!currentValues.Contains(component))
                        {
                            currentValues.Add(component);
                            currentType.Optional = string.Join(",", currentValues.ToArray());
                            await _resourceTypeService.PostItem(currentType);
                        }

                        break;
                    case "optional-remove":
                        currentValues = new List<string>(currentType.Optional.Split(','));
                        if (currentValues.Contains(component))
                        {
                            currentValues.Remove(component);
                            currentType.Optional = string.Join(",", currentValues.ToArray());
                            await _resourceTypeService.PostItem(currentType);
                        }

                        break;
                    case "exclude-add":
                        currentValues = new List<string>(currentType.Exclude.Split(','));
                        if (!currentValues.Contains(component))
                        {
                            currentValues.Add(component);
                            currentType.Exclude = string.Join(",", currentValues.ToArray());
                            await  _resourceTypeService.PostItem(currentType);
                        }

                        break;
                    case "exclude-remove":
                        currentValues = new List<string>(currentType.Exclude.Split(','));
                        if (currentValues.Contains(component))
                        {
                            currentValues.Remove(component);
                            currentType.Exclude = string.Join(",", currentValues.ToArray());
                            await _resourceTypeService.PostItem(currentType);
                        }

                        break;
                }
            }

            serviceResponse.ResponseObject = "Resource Types updated!";
            serviceResponse.Success = true;
        }
        else
        {
            serviceResponse.ResponseObject = "Resource Types not found!";
        }

        return serviceResponse;
    }
}