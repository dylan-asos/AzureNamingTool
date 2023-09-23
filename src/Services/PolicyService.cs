using AzureNamingTool.Helpers;
using AzureNamingTool.Models;

namespace AzureNamingTool.Services;

public class PolicyService
{
    private readonly FileReader _fileReader;

    public PolicyService(
        FileReader reader)
    {
        _fileReader = reader;
    }

    public ServiceResponse GetPolicy()
    {
        ServiceResponse serviceResponse = new();

        var delimiter = '-';
        var nameComponents = _fileReader.GetList<ResourceComponent>();
        var resourceTypes = _fileReader.GetList<ResourceType>();
        var unitDepts = _fileReader.GetList<ResourceUnitDept>();
        var environments = _fileReader.GetList<ResourceEnvironment>();
        var locations = _fileReader.GetList<ResourceLocation>();
        var orgs = _fileReader.GetList<ResourceOrg>();
        var functions = _fileReader.GetList<ResourceFunction>();
        var projectAppSvcs = _fileReader.GetList<ResourceProjAppSvc>();

        List<string> validations = new();
        var maxSortOrder = 0;
        if (nameComponents != null)
        {
            foreach (var nameComponent in nameComponents)
            {
                var name = (string) nameComponent.Name;
                var isEnabled = nameComponent.Enabled;
                var sortOrder = nameComponent.SortOrder;
                maxSortOrder = sortOrder - 1;
                if (isEnabled)
                {
                    switch (name)
                    {
                        case "ResourceType":
                            if (resourceTypes != null)
                            {
                                AddValidations(resourceTypes, validations, delimiter, sortOrder);
                            }

                            break;
                        case "ResourceUnitDept":
                            if (unitDepts != null)
                            {
                                AddValidations(unitDepts, validations, delimiter, sortOrder);
                            }

                            break;
                        case "ResourceEnvironment":
                            if (environments != null)
                            {
                                AddValidations(environments, validations, delimiter, sortOrder);
                            }

                            break;
                        case "ResourceLocation":
                            if (locations != null)
                            {
                                AddValidations(locations, validations, delimiter, sortOrder);
                            }

                            break;
                        case "ResourceOrgs":
                            if (orgs != null)
                            {
                                AddValidations(orgs, validations, delimiter, sortOrder);
                            }

                            break;
                        case "ResourceFunctions":
                            if (functions != null)
                            {
                                AddValidations(functions, validations, delimiter, sortOrder);
                            }

                            break;
                        case "ResourceProjAppSvcs":
                            if (projectAppSvcs != null)
                            {
                                AddValidations(projectAppSvcs, validations, delimiter, sortOrder);
                            }

                            break;
                    }
                }
            }
        }

        var property = new PolicyProperty("Name Validation",
            "This policy enables you to restrict the name can be specified when deploying a Azure Resource.")
            {
                PolicyRule = PolicyRuleFactory.GetNameValidationRules(validations.Select(x => new PolicyRule(x, delimiter)).ToList(),
                    delimiter)
            };
        PolicyDefinition definition = new(property);

        //serviceResponse.ResponseObject = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(definition.ToString())).ToArray();
        serviceResponse.ResponseObject = definition;
        serviceResponse.Success = true;

        return serviceResponse;
    }

    private static void AddValidations(dynamic list, List<string> validations, char delimiter, int level)
    {
        if (validations.Count == 0)
        {
            foreach (var item in list)
            {
                if (item.ShortName.Trim() != string.Empty)
                {
                    var key = $"{item.ShortName}{delimiter}*";
                    if (!validations.Contains(key))
                        validations.Add(key);
                }
            }
        }
        else
        {
            foreach (var item in list)
            {
                if (item.ShortName.Trim() != string.Empty)
                {
                    foreach (var validation in validations
                                 .Where(x => x.Count(p => p.ToString().Contains(delimiter)) == level - 1).ToList())
                    {
                        var key = $"{validation.Replace("*", "")}{item.ShortName}{delimiter}*";
                        if (!validations.Contains(key))
                            validations.Add(key);
                    }
                }
            }
        }
    }
}