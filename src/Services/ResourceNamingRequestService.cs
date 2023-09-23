using System.Text;
using AzureNamingTool.Helpers;
using AzureNamingTool.Models;

namespace AzureNamingTool.Services;

public class ResourceNamingRequestService
{
    private readonly ConfigurationHelper _configurationHelper;
    private readonly CustomComponentService _customComponentService;
    private readonly GeneratedNamesService _generatedNamesService;
    private readonly ResourceComponentService _resourceComponentService;
    private readonly ResourceDelimiterService _resourceDelimiterService;
    private readonly ResourceTypeService _resourceTypeService;
    private readonly GeneralHelper _generalHelper;
    private readonly AdminLogService _adminLogService;
    private readonly FileReader _fileReader;

    public ResourceNamingRequestService(
        ResourceTypeService resourceTypeService,
        GeneratedNamesService generatedNamesService,
        ResourceComponentService resourceComponentService,
        ResourceDelimiterService resourceDelimiterService,
        ConfigurationHelper configurationHelper,
        CustomComponentService customComponentService, 
        GeneralHelper generalHelper, 
        AdminLogService adminLogService, 
        FileReader fileReader)
    {
        _resourceTypeService = resourceTypeService;
        _generatedNamesService = generatedNamesService;
        _resourceComponentService = resourceComponentService;
        _resourceDelimiterService = resourceDelimiterService;
        _configurationHelper = configurationHelper;
        _customComponentService = customComponentService;
        _generalHelper = generalHelper;
        _adminLogService = adminLogService;
        _fileReader = fileReader;
    }

    /// <summary>
    ///     This function will generate a resoure type name for specifed component values. This function requires full
    ///     definition for all components. It is recommended to use the ResourceNameRequest API function for name generation.
    /// </summary>
    /// <param name="request"></param>
    /// <returns>ResourceNameResponse - Response of name generation</returns>
    public ResourceNameResponse RequestNameWithComponents(ResourceNameRequestWithComponents request)
    {
        ResourceNameResponse response = new()
        {
            Success = false
        };

        try
        {
            var valid = true;
            var ignoreDelimeter = false;
            List<string[]> lstComponents = new();

            // Get the specified resource type
            //var resourceTypes = await ConfigurationHelper.GetList<ResourceType>();
            //var resourceType = resourceTypes.Find(x => x.Id == request.ResourceType);
            var resourceType = request.ResourceType;

            // Check static value
            if (!string.IsNullOrEmpty(resourceType.StaticValues))
            {
                // Return the static value and message and stop generation.
                response.ResourceName = resourceType.StaticValues;
                response.Message =
                    "The requested Resource Type name is considered a static value with specific requirements. Please refer to https://docs.microsoft.com/en-us/azure/azure-resource-manager/management/resource-name-rules for additional information.";
                response.Success = true;
                return response;
            }

            // Get the components
            ServiceResponse serviceresponse = new();
            serviceresponse = _resourceComponentService.GetItems(false);
            var currentResourceComponents = serviceresponse.ResponseObject;
            dynamic d = request;

            var name = "";

            StringBuilder sbMessage = new();

            // Loop through each component
            if (currentResourceComponents != null)
            {
                foreach (var component in currentResourceComponents!)
                {
                    string normalizedcomponentname = _generalHelper.NormalizeName(component.Name, true);
                    // Check if the component is excluded for the Resource Type
                    if (!resourceType.Exclude.ToLower().Split(',').Contains(normalizedcomponentname))
                    {
                        // Attempt to retrieve value from JSON body
                        var prop = _generalHelper.GetPropertyValue(d, component.Name);
                        var value = string.Empty;

                        // Add property value to name, if exists
                        if (prop!= null)
                        {
                            if (component.Name == "ResourceInstance")
                            {
                                value = prop;
                            }
                            else
                            {
                                value = prop.GetType().GetProperty("ShortName").GetValue(prop, null).ToLower();
                            }

                            // Check if the delimeter is already ignored
                            if (!ignoreDelimeter)
                            {
                                // Check if delimeter is an invalid character
                                if (!string.IsNullOrEmpty(resourceType.InvalidCharacters))
                                {
                                    if (!resourceType.InvalidCharacters.Contains(request.ResourceDelimiter.Delimiter))
                                    {
                                        if (!string.IsNullOrEmpty(name))
                                        {
                                            name += request.ResourceDelimiter.Delimiter;
                                        }
                                    }
                                    else
                                    {
                                        // Add message about delimeter not applied
                                        sbMessage.Append(
                                            "The specified delimiter is not allowed for this resource type and has been removed.");
                                        sbMessage.Append(Environment.NewLine);
                                        ignoreDelimeter = true;
                                    }
                                }
                                else
                                {
                                    // Deliemeter is valid so add it
                                    if (!string.IsNullOrEmpty(name))
                                    {
                                        name += request.ResourceDelimiter.Delimiter;
                                    }
                                }
                            }

                            name += value;

                            // Add property to aray for indivudal component validation
                            if (component.Name == "ResourceType")
                            {
                                lstComponents.Add(new string[] {component.Name, prop.Resource + " (" + value + ")"});
                            }
                            else
                            {
                                if (component.Name == "ResourceInstance")
                                {
                                    lstComponents.Add(new string[] {component.Name, prop});
                                }
                                else
                                {
                                    lstComponents.Add(new string[] {component.Name, prop.Name + " (" + value + ")"});
                                }
                            }
                        }
                        else
                        {
                            // Check if the prop is optional
                            if (!resourceType.Optional.ToLower().Split(',').Contains(normalizedcomponentname))
                            {
                                valid = false;
                                break;
                            }
                        }
                    }
                }
            }

            // Check if the required component were supplied
            if (!valid)
            {
                response.ResourceName = "***RESOURCE NAME NOT GENERATED***";
                response.Message = "You must supply the required components.";
                return response;
            }

            // Check the Resource Instance value to ensure it's only nmumeric
            if (lstComponents.FirstOrDefault(x => x[0] == "ResourceInstance")!= null)
            {
                if (lstComponents.FirstOrDefault(x => x[0] == "ResourceInstance")![1]!= null)
                {
                    if (!ValidationHelper.CheckNumeric(
                            lstComponents.FirstOrDefault(x => x[0] == "ResourceInstance")![1]))
                    {
                        sbMessage.Append("Resource Instance must be a numeric value.");
                        sbMessage.Append(Environment.NewLine);
                        valid = false;
                    }
                }
            }

            // Validate the generated name for the resource type
            // CALL VALIDATION FUNCTION
            var validateNameRequest = new ValidateNameRequest
            {
                ResourceType = resourceType.ShortName,
                Name = name
            };
            var serviceResponse = _resourceTypeService.ValidateResourceTypeName(validateNameRequest);
            if (serviceResponse.Success)
            {
                if (serviceResponse.ResponseObject != null)
                {
                    var validateNameResponse = (ValidateNameResponse) serviceResponse.ResponseObject!;
                    valid = validateNameResponse.Valid;
                    if (!string.IsNullOrEmpty(validateNameResponse.Name))
                    {
                        name = validateNameResponse.Name;
                    }

                    if (!string.IsNullOrEmpty(validateNameResponse.Message))
                    {
                        sbMessage.Append(validateNameResponse.Message);
                    }
                }
            }

            if (valid)
            {
                GeneratedName generatedName = new()
                {
                    CreatedOn = DateTime.Now,
                    ResourceName = name.ToLower(),
                    Components = lstComponents
                };
                _generatedNamesService.PostItem(generatedName);
                response.Success = true;
                response.ResourceName = name.ToLower();
                response.Message = sbMessage.ToString();
                return response;
            }

            response.ResourceName = "***RESOURCE NAME NOT GENERATED***";
            response.Message = sbMessage.ToString();
            return response;
        }
        catch (Exception ex)
        {
            _adminLogService.PostItem(new AdminLogMessage {Title = "ERROR", Message = ex.Message});
            response.Message = ex.Message;
            return response;
        }
    }

    /// <summary>
    ///     This function is used to generate a name by providing each componetn and the short name value. The function will
    ///     validate the values to ensure they match the current configuration.
    /// </summary>
    /// <param name="request"></param>
    /// <returns>ResourceNameResponse - Response of name generation</returns>
    public async Task<ResourceNameResponse> RequestName(ResourceNameRequest request)
    {
        ResourceNameResponse resourceNameResponse = new()
        {
            Success = false
        };


        var valid = true;
        var ignoredelimeter = false;
        List<string[]> lstComponents = new();
        ServiceResponse serviceResponse = new();
        ResourceDelimiter resourceDelimiter = new();
        ResourceType resourceType = new();
        var name = "";
        StringBuilder sbMessage = new();

        // Get the current delimiter
        serviceResponse = _resourceDelimiterService.GetCurrentItem();
        if (serviceResponse.Success)
        {
            resourceDelimiter = (ResourceDelimiter) serviceResponse.ResponseObject!;
        }
        else
        {
            valid = false;
            resourceNameResponse.Message = "Delimiter value could not be set.";
            resourceNameResponse.Success = false;
            return resourceNameResponse;
        }

        // Get the specified resource type
        var resourceTypes = _fileReader.GetList<ResourceType>();
        if (resourceTypes!= null)
        {
            var resourceTypesByShortName = resourceTypes.FindAll(x => x.ShortName == request.ResourceType);
            if (resourceTypesByShortName == null)
            {
                resourceNameResponse.Message = "ResourceType value is invalid.";
                resourceNameResponse.Success = false;
                return resourceNameResponse;
            }

            if (resourceTypesByShortName.Count == 0)
            {
                resourceNameResponse.Message = "ResourceType value is invalid.";
                resourceNameResponse.Success = false;
                return resourceNameResponse;
            }

            // Check if there are duplicates
            if (resourceTypesByShortName.Count > 1)
            {
                // Check that the request includes a resource name
                if (request.ResourceId != 0)
                {
                    // Check if the resource value is valid
                    resourceType = resourceTypesByShortName.Find(x => x.Id == request.ResourceId)!;
                    if (resourceType == null)
                    {
                        resourceNameResponse.Message = "Resource Id value is invalid.";
                        resourceNameResponse.Success = false;
                        return resourceNameResponse;
                    }
                }
                else
                {
                    resourceNameResponse.Message =
                        "Your configuration contains multiple resource types for the provided short name. You must supply the Resource Id value for the resource type in your request.(Example: resourceId: 14)";
                    resourceNameResponse.Success = false;
                    return resourceNameResponse;
                }
            }
            else
            {
                // Set the resource type ot the first value
                resourceType = resourceTypesByShortName[0];
            }

            // Check static value
            if (!string.IsNullOrEmpty(resourceType.StaticValues))
            {
                // Return the static value and message and stop generation.
                resourceNameResponse.ResourceName = resourceType.StaticValues;
                resourceNameResponse.Message =
                    "The requested Resource Type name is considered a static value with specific requirements. Please refer to https://docs.microsoft.com/en-us/azure/azure-resource-manager/management/resource-name-rules for additional information.";
                resourceNameResponse.Success = true;
                return resourceNameResponse;
            }

            // Make sure the passed custom component names are normalized
            if (request.CustomComponents!= null)
            {
                Dictionary<string, string> newComponents = new();
                foreach (var cc in request.CustomComponents)
                {
                    var value = cc.Value;
                    newComponents.Add(_generalHelper.NormalizeName(cc.Key, true), value);
                }

                request.CustomComponents = newComponents;
            }

            // Get the current components
            serviceResponse = _resourceComponentService.GetItems(false);
            if (serviceResponse.Success)
            {
                if (serviceResponse.ResponseObject != null)
                {
                    var currentResourceComponents = serviceResponse.ResponseObject;
                    if (currentResourceComponents != null)
                    {
                        // Loop through each component
                        foreach (var component in currentResourceComponents!)
                        {
                            string normalizedcomponentname = _generalHelper.NormalizeName(component.Name, true);
                            if (!component.IsCustom)
                            {
                                // Check if the component is excluded for the Resource Type
                                if (!resourceType.Exclude.ToLower().Split(',').Contains(normalizedcomponentname))
                                {
                                    // Attempt to retrieve value from JSON body
                                    var value = _generalHelper.GetPropertyValue(request, component.Name);

                                    // Add property value to name, if exists
                                    if (value != null)
                                    {
                                        if (!string.IsNullOrEmpty(value))
                                        {
                                            // Validate that the value is a valid option for the component
                                            switch (component.Name.ToLower())
                                            {
                                                case "resourcetype":
                                                    var types = _fileReader.GetList<ResourceType>();
                                                    if (types!= null)
                                                    {
                                                        var type = types.Find(x => x.ShortName == value);
                                                        if (type == null)
                                                        {
                                                            valid = false;
                                                            sbMessage.Append("ResourceType value is invalid. ");
                                                        }
                                                    }

                                                    break;
                                                case "resourceenvironment":
                                                    var environments =
                                                        _fileReader.GetList<ResourceEnvironment>();
                                                    if (environments!= null)
                                                    {
                                                        var environment = environments.Find(x => x.ShortName == value);
                                                        if (environment == null)
                                                        {
                                                            valid = false;
                                                            sbMessage.Append("ResourceEnvironment value is invalid. ");
                                                        }
                                                    }

                                                    break;
                                                case "resourcelocation":
                                                    var locations =
                                                        _fileReader.GetList<ResourceLocation>();
                                                    if (locations != null)
                                                    {
                                                        var location = locations.Find(x => x.ShortName == value);
                                                        if (location== null)
                                                        {
                                                            valid = false;
                                                            sbMessage.Append("ResourceLocation value is invalid. ");
                                                        }
                                                    }

                                                    break;
                                                case "resourceorg":
                                                    var orgs = _fileReader.GetList<ResourceOrg>();
                                                    if (orgs!= null)
                                                    {
                                                        var org = orgs.Find(x => x.ShortName == value);
                                                        if (org == null)
                                                        {
                                                            valid = false;
                                                            sbMessage.Append("Resource Type value is invalid. ");
                                                        }
                                                    }

                                                    break;
                                                case "resourceprojappsvc":
                                                    var projappsvcs =
                                                        _fileReader.GetList<ResourceProjAppSvc>();
                                                    if (projappsvcs!= null)
                                                    {
                                                        var projappsvc = projappsvcs.Find(x => x.ShortName == value);
                                                        if (projappsvc == null)
                                                        {
                                                            valid = false;
                                                            sbMessage.Append("ResourceProjAppSvc value is invalid. ");
                                                        }
                                                    }

                                                    break;
                                                case "resourceunitdept":
                                                    var unitdepts =
                                                        _fileReader.GetList<ResourceUnitDept>();
                                                    if (unitdepts!= null)
                                                    {
                                                        var unitdept = unitdepts.Find(x => x.ShortName == value);
                                                        if (unitdept == null)
                                                        {
                                                            valid = false;
                                                            sbMessage.Append("ResourceUnitDept value is invalid. ");
                                                        }
                                                    }

                                                    break;
                                                case "resourcefunction":
                                                    var functions =
                                                        _fileReader.GetList<ResourceFunction>();
                                                    if (functions!= null)
                                                    {
                                                        var function = functions.Find(x => x.ShortName == value);
                                                        if (function == null)
                                                        {
                                                            valid = false;
                                                            sbMessage.Append("ResourceFunction value is invalid. ");
                                                        }
                                                    }

                                                    break;
                                            }

                                            // Check if the delimeter is already ignored
                                            if (!ignoredelimeter && !string.IsNullOrEmpty(resourceDelimiter.Delimiter))
                                            {
                                                // Check if delimeter is an invalid character
                                                if (!string.IsNullOrEmpty(resourceType.InvalidCharacters))
                                                {
                                                    if (!resourceType.InvalidCharacters.Contains(resourceDelimiter
                                                            .Delimiter))
                                                    {
                                                        if (name != "")
                                                        {
                                                            name += resourceDelimiter.Delimiter;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        // Add message about delimeter not applied
                                                        sbMessage.Append(
                                                            "The specified delimiter is not allowed for this resource type and has been removed. ");
                                                        ignoredelimeter = true;
                                                    }
                                                }
                                                else
                                                {
                                                    // Deliemeter is valid so add it
                                                    if (!string.IsNullOrEmpty(name))
                                                    {
                                                        name += resourceDelimiter.Delimiter;
                                                    }
                                                }
                                            }

                                            name += value;

                                            // Add property to array for individual component validation
                                            if (!resourceType.Exclude.ToLower().Split(',')
                                                    .Contains(normalizedcomponentname))
                                            {
                                                lstComponents.Add(new string[] {component.Name, value});
                                            }
                                        }
                                        else
                                        {
                                            // Check if the prop is optional
                                            if (!resourceType.Optional.ToLower().Split(',')
                                                    .Contains(normalizedcomponentname))
                                            {
                                                valid = false;
                                                sbMessage.Append(component.Name + " value was not provided. ");
                                            }
                                        }
                                    }
                                    else
                                    {
                                        // Check if the prop is optional
                                        if (!resourceType.Optional.ToLower().Split(',')
                                                .Contains(normalizedcomponentname))
                                        {
                                            valid = false;
                                            sbMessage.Append(component.Name + " value was not provided. ");
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (!component.IsFreeText)
                                {
                                    // Get the custom components data
                                    serviceResponse = _customComponentService.GetItems();
                                    if (serviceResponse.Success)
                                    {
                                        if (serviceResponse.ResponseObject != null)
                                        {
                                            if (serviceResponse.ResponseObject != null)
                                            {
                                                var customcomponents =
                                                    (List<CustomComponent>) serviceResponse.ResponseObject!;
                                                if (customcomponents!= null)
                                                {
                                                    // Make sure the custom component has values
                                                    if (customcomponents.Any(x => x.ParentComponent == normalizedcomponentname))
                                                    {
                                                        // Make sure the CustomComponents property was provided
                                                        if (!resourceType.Exclude.ToLower().Split(',')
                                                                .Contains(normalizedcomponentname))
                                                        {
                                                            // Add property value to name, if exists
                                                            if (request.CustomComponents!= null)
                                                            {
                                                                // Check if the custom compoment value was provided in the request
                                                                if (request.CustomComponents.ContainsKey(
                                                                        normalizedcomponentname))
                                                                {
                                                                    // Get the value from the provided custom components
                                                                    var componentvalue =
                                                                        request.CustomComponents[
                                                                            normalizedcomponentname];
                                                                    if (componentvalue == null)
                                                                    {
                                                                        // Check if the prop is optional
                                                                        if (!resourceType.Optional.ToLower().Split(',')
                                                                                .Contains(normalizedcomponentname))
                                                                        {
                                                                            valid = false;
                                                                            sbMessage.Append(component.Name +
                                                                                " value was not provided. ");
                                                                        }
                                                                    }
                                                                    else
                                                                    {
                                                                        // Check to make sure it is a valid custom component
                                                                        var customComponents =
                                                                            _fileReader
                                                                                .GetList<CustomComponent>();
                                                                        if (customComponents!= null)
                                                                        {
                                                                            var validcustomComponent =
                                                                                customComponents.Find(x =>
                                                                                    x.ParentComponent ==
                                                                                    normalizedcomponentname &&
                                                                                    x.ShortName == componentvalue);
                                                                            if (validcustomComponent == null)
                                                                            {
                                                                                valid = false;
                                                                                sbMessage.Append(component.Name +
                                                                                    " value is not a valid custom component short name. ");
                                                                            }
                                                                            else
                                                                            {
                                                                                if (!string.IsNullOrEmpty(name))
                                                                                {
                                                                                    name += resourceDelimiter.Delimiter;
                                                                                }

                                                                                name += componentvalue;

                                                                                // Add property to array for individual component validation
                                                                                lstComponents.Add(new string[]
                                                                                    {component.Name, componentvalue});
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                                else
                                                                {
                                                                    // Check if the prop is optional
                                                                    if (!resourceType.Optional.ToLower().Split(',')
                                                                            .Contains(normalizedcomponentname))
                                                                    {
                                                                        valid = false;
                                                                        sbMessage.Append(component.Name +
                                                                            " value was not provided. ");
                                                                    }
                                                                }
                                                            }
                                                            else
                                                            {
                                                                // Check if the prop is optional
                                                                if (!resourceType.Optional.ToLower().Split(',')
                                                                        .Contains(normalizedcomponentname))
                                                                {
                                                                    valid = false;
                                                                    sbMessage.Append(component.Name +
                                                                        " value was not provided. ");
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    // Make sure the CustomComponents property was provided
                                    if (!resourceType.Exclude.ToLower().Split(',').Contains(normalizedcomponentname))
                                    {
                                        // Add property value to name, if exists
                                        if (request.CustomComponents!= null)
                                        {
                                            // Check if the custom compoment value was provided in the request
                                            if (request.CustomComponents.ContainsKey(normalizedcomponentname))
                                            {
                                                // Get the value from the provided custom components
                                                var componentvalue = request.CustomComponents[normalizedcomponentname];
                                                if (componentvalue == null)
                                                {
                                                    // Check if the prop is optional
                                                    if (!resourceType.Optional.ToLower().Split(',')
                                                            .Contains(normalizedcomponentname))
                                                    {
                                                        valid = false;
                                                        sbMessage.Append(component.Name + " value was not provided. ");
                                                    }
                                                }
                                                else
                                                {
                                                    if (!string.IsNullOrEmpty(name))
                                                    {
                                                        name += resourceDelimiter.Delimiter;
                                                    }

                                                    name += componentvalue;

                                                    // Add property to array for individual component validation
                                                    lstComponents.Add(new string[] {component.Name, componentvalue});
                                                }
                                            }
                                            else
                                            {
                                                // Check if the prop is optional
                                                if (!resourceType.Optional.ToLower().Split(',')
                                                        .Contains(normalizedcomponentname))
                                                {
                                                    valid = false;
                                                    sbMessage.Append(component.Name + " value was not provided. ");
                                                }
                                            }
                                        }
                                        else
                                        {
                                            // Check if the prop is optional
                                            if (!resourceType.Optional.ToLower().Split(',')
                                                    .Contains(normalizedcomponentname))
                                            {
                                                valid = false;
                                                sbMessage.Append(component.Name + " value was not provided. ");
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        else
        {
            valid = false;
            sbMessage.Append("There was a problem generating the name.");
        }

        // Check if the required component were supplied
        if (!valid)
        {
            resourceNameResponse.ResourceName = "***RESOURCE NAME NOT GENERATED***";
            resourceNameResponse.Message = "You must supply the required components. " + sbMessage;
            return resourceNameResponse;
        }

        // Check the Resource Instance value to ensure it's only nmumeric
        if (lstComponents!= null)
        {
            if (lstComponents.FirstOrDefault(x => x[0] == "ResourceInstance")!= null)
            {
                if (lstComponents.FirstOrDefault(x => x[0] == "ResourceInstance")![1]!= null)
                {
                    if (!ValidationHelper.CheckNumeric(
                            lstComponents.FirstOrDefault(x => x[0] == "ResourceInstance")![1]))
                    {
                        sbMessage.Append("Resource Instance must be a numeric value.");
                        sbMessage.Append(Environment.NewLine);
                        valid = false;
                    }
                }
            }
        }

        // Validate the generated name for the resource type
        var validateNameRequest = new ValidateNameRequest
        {
            ResourceType = resourceType.ShortName,
            Name = name
        };
        serviceResponse = _resourceTypeService.ValidateResourceTypeName(validateNameRequest);
        if (serviceResponse.Success)
        {
            if (serviceResponse.ResponseObject != null)
            {
                var validateNameResponse = (ValidateNameResponse) serviceResponse.ResponseObject!;
                valid = validateNameResponse.Valid;
                if (!string.IsNullOrEmpty(validateNameResponse.Name))
                {
                    name = validateNameResponse.Name;
                }

                if (!string.IsNullOrEmpty(validateNameResponse.Message))
                {
                    sbMessage.Append(validateNameResponse.Message);
                }
            }
        }

        if (valid)
        {
            var nameallowed = true;
            // Check if duplicate names are allowed
            if (!_configurationHelper.VerifyDuplicateNamesAllowed())
            {
                // Check if the name already exists
                serviceResponse = _generatedNamesService.GetItems();
                if (serviceResponse.Success)
                {
                    if (serviceResponse.ResponseObject != null)
                    {
                        var names = (List<GeneratedName>) serviceResponse.ResponseObject!;
                        if (names!= null)
                        {
                            if (names.Any(x => x.ResourceName == name))
                            {
                                nameallowed = false;
                            }
                        }
                    }
                }
            }

            if (nameallowed)
            {
                GeneratedName generatedName = new()
                {
                    CreatedOn = DateTime.Now,
                    ResourceName = name.ToLower(),
                    Components = lstComponents,
                    ResourceTypeName = resourceType.Resource,
                    User = request.CreatedBy
                };

                // Check if the property should be appended to name
                if (!string.IsNullOrEmpty(resourceType.Property))
                {
                    generatedName.ResourceTypeName += " - " + resourceType.Property;
                }

                var responseGenerateName = _generatedNamesService.PostItem(generatedName);
                if (responseGenerateName.Success)
                {
                    resourceNameResponse.Success = true;
                    resourceNameResponse.ResourceName = name.ToLower();
                    resourceNameResponse.Message = sbMessage.ToString();
                    resourceNameResponse.ResourceNameDetails = generatedName;

                    // Check if the GenerationWebhook is configured
                    var webhook = _configurationHelper.GetAppSetting("GenerationWebhook", true);
                    if (!string.IsNullOrEmpty(webhook))
                    {
                        // Asynchronously post to the webhook
                        await _configurationHelper.PostToGenerationWebhook(webhook, generatedName);
                    }
                }
                else
                {
                    resourceNameResponse.Success = false;
                    resourceNameResponse.ResourceName = "***RESOURCE NAME NOT GENERATED***";
                    resourceNameResponse.Message = "There was an error generating the name. Please try again.";
                }
            }
            else
            {
                resourceNameResponse.Success = false;
                resourceNameResponse.ResourceName = "***RESOURCE NAME NOT GENERATED***";
                resourceNameResponse.Message = "The name (" + name +
                                               ") you are trying to generate already exists. Please select different component options and try again.";
            }

            return resourceNameResponse;
        }

        resourceNameResponse.ResourceName = "***RESOURCE NAME NOT GENERATED***";
        resourceNameResponse.Message = sbMessage.ToString();
        return resourceNameResponse;
    }
}