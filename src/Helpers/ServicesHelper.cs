using AzureNamingTool.Models;
using AzureNamingTool.Services;

namespace AzureNamingTool.Helpers;

public class ServicesHelper
{
    private readonly AdminLogService _adminLogService;
    private readonly AdminUserService _adminUserService;
    private readonly CustomComponentService _customComponentService;
    private readonly GeneratedNamesService _generatedNamesService;
    private readonly ResourceComponentService _resourceComponentService;
    private readonly ResourceDelimiterService _resourceDelimiterService;
    private readonly ResourceEnvironmentService _resourceEnvironmentService;
    private readonly ResourceFunctionService _resourceFunctionService;
    private readonly ResourceLocationService _resourceLocationService;
    private readonly ResourceOrgService _resourceOrgService;
    private readonly ResourceProjAppSvcService _resourceProjAppSvcService;
    private readonly ResourceTypeService _resourceTypeService;
    private readonly ResourceUnitDeptService _resourceUnitDeptService;

    public ServicesHelper(
        ResourceComponentService resourceComponentService,
        ResourceDelimiterService resourceDelimiterService,
        ResourceLocationService resourceLocationService,
        ResourceOrgService resourceOrgService,
        ResourceProjAppSvcService resourceProjAppSvcService,
        ResourceTypeService resourceTypeService,
        ResourceUnitDeptService resourceUnitDeptService,
        ResourceEnvironmentService resourceEnvironmentService,
        ResourceFunctionService resourceFunctionService,
        CustomComponentService customComponentService,
        GeneratedNamesService generatedNamesService,
        AdminLogService adminLogService,
        AdminUserService adminUserService)
    {
        _resourceComponentService = resourceComponentService;
        _resourceDelimiterService = resourceDelimiterService;
        _resourceLocationService = resourceLocationService;
        _resourceOrgService = resourceOrgService;
        _resourceProjAppSvcService = resourceProjAppSvcService;
        _resourceTypeService = resourceTypeService;
        _resourceUnitDeptService = resourceUnitDeptService;
        _resourceEnvironmentService = resourceEnvironmentService;
        _resourceFunctionService = resourceFunctionService;
        _customComponentService = customComponentService;
        _generatedNamesService = generatedNamesService;
        _adminLogService = adminLogService;
        _adminUserService = adminUserService;
    }


    public async Task<ServicesData> LoadServicesData(bool admin)
    {
        ServicesData servicesData = new();
        var serviceResponse = await _resourceComponentService.GetItems(admin);
        servicesData.ResourceComponents = (List<ResourceComponent>?) serviceResponse.ResponseObject;
        serviceResponse = await _resourceDelimiterService.GetItems(admin);
        servicesData.ResourceDelimiters = (List<ResourceDelimiter>?) serviceResponse.ResponseObject;
        serviceResponse = await _resourceEnvironmentService.GetItems();
        servicesData.ResourceEnvironments = (List<ResourceEnvironment>?) serviceResponse.ResponseObject;
        serviceResponse = await _resourceLocationService.GetItems(admin);
        servicesData.ResourceLocations = (List<ResourceLocation>?) serviceResponse.ResponseObject;
        serviceResponse = await _resourceOrgService.GetItems();
        servicesData.ResourceOrgs = (List<ResourceOrg>?) serviceResponse.ResponseObject;
        serviceResponse = await _resourceProjAppSvcService.GetItems();
        servicesData.ResourceProjAppSvcs = (List<ResourceProjAppSvc>?) serviceResponse.ResponseObject;
        serviceResponse = await _resourceTypeService.GetItems(admin);
        servicesData.ResourceTypes = (List<ResourceType>?) serviceResponse.ResponseObject;
        serviceResponse = await _resourceUnitDeptService.GetItems();
        servicesData.ResourceUnitDepts = (List<ResourceUnitDept>?) serviceResponse.ResponseObject;
        serviceResponse = await _resourceFunctionService.GetItems();
        servicesData.ResourceFunctions = (List<ResourceFunction>?) serviceResponse.ResponseObject;
        serviceResponse = await _customComponentService.GetItems();
        servicesData.CustomComponents = (List<CustomComponent>?) serviceResponse.ResponseObject;
        serviceResponse = await _generatedNamesService.GetItems();
        servicesData.GeneratedNames = (List<GeneratedName>?) serviceResponse.ResponseObject;
        serviceResponse = await _adminLogService.GetItems();
        servicesData.AdminLogMessages = (List<AdminLogMessage>?) serviceResponse.ResponseObject;
        serviceResponse = await _adminUserService.GetItems();
        servicesData.AdminUsers = (List<AdminUser>?) serviceResponse.ResponseObject;
        return servicesData;
    }
}