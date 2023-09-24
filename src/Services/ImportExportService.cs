using System.Text.Json;
using System.Text.Json.Serialization;
using AzureNamingTool.Helpers;
using AzureNamingTool.Models;

namespace AzureNamingTool.Services;

public class ImportExportService
{
    private readonly AdminLogService _adminLogService;
    private readonly AdminUserService _adminUserService;
    private readonly CacheHelper _cacheHelper;
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
    private readonly SiteConfiguration _siteConfiguration;

    public ImportExportService(
        CacheHelper cacheHelper,
        ResourceComponentService resourceComponentService,
        ResourceDelimiterService resourceDelimiterService,
        ResourceEnvironmentService resourceEnvironmentService,
        ResourceFunctionService resourceFunctionService,
        ResourceLocationService resourceLocationService,
        ResourceOrgService resourceOrgService,
        ResourceProjAppSvcService resourceProjAppSvcService,
        ResourceTypeService resourceTypeService,
        ResourceUnitDeptService resourceUnitDeptService,
        CustomComponentService customComponentService,
        GeneratedNamesService generatedNamesService,
        AdminLogService adminLogService,
        AdminUserService adminUserService, SiteConfiguration siteConfiguration)
    {
        _cacheHelper = cacheHelper;
        _resourceComponentService = resourceComponentService;
        _resourceDelimiterService = resourceDelimiterService;
        _resourceEnvironmentService = resourceEnvironmentService;
        _resourceFunctionService = resourceFunctionService;
        _resourceLocationService = resourceLocationService;
        _resourceOrgService = resourceOrgService;
        _resourceProjAppSvcService = resourceProjAppSvcService;
        _resourceTypeService = resourceTypeService;
        _resourceUnitDeptService = resourceUnitDeptService;
        _customComponentService = customComponentService;
        _generatedNamesService = generatedNamesService;
        _adminLogService = adminLogService;
        _adminUserService = adminUserService;
        _siteConfiguration = siteConfiguration;
    }

    public ServiceResponse ExportConfig(bool includeadmin = false)
    {
        ServiceResponse serviceResponse = new();

        ConfigurationData configdata = new();
        // Get the current data
        //ResourceComponents
        serviceResponse = _resourceComponentService.GetItems(true);
        if (serviceResponse.Success)
        {
            if (serviceResponse.ResponseObject != null)
            {
                configdata.ResourceComponents = serviceResponse.ResponseObject!;
            }
        }

        //ResourceDelimiters
        serviceResponse = _resourceDelimiterService.GetItems(true);
        if (serviceResponse.ResponseObject != null)
        {
            configdata.ResourceDelimiters = serviceResponse.ResponseObject!;
        }

        //ResourceEnvironments
        serviceResponse = _resourceEnvironmentService.GetItems();
        if (serviceResponse.ResponseObject != null)
        {
            configdata.ResourceEnvironments = serviceResponse.ResponseObject!;
        }

        // ResourceFunctions
        serviceResponse = _resourceFunctionService.GetItems();
        if (serviceResponse.ResponseObject != null)
        {
            configdata.ResourceFunctions = serviceResponse.ResponseObject!;
        }

        // ResourceLocations
        serviceResponse = _resourceLocationService.GetItems();
        if (serviceResponse.ResponseObject != null)
        {
            configdata.ResourceLocations = serviceResponse.ResponseObject!;
        }

        // ResourceOrgs
        serviceResponse = _resourceOrgService.GetItems();
        if (serviceResponse.ResponseObject != null)
        {
            configdata.ResourceOrgs = serviceResponse.ResponseObject!;
        }

        // ResourceProjAppSvc
        serviceResponse = _resourceProjAppSvcService.GetItems();
        if (serviceResponse.ResponseObject != null)
        {
            configdata.ResourceProjAppSvcs = serviceResponse.ResponseObject!;
        }

        // ResourceTypes
        serviceResponse = _resourceTypeService.GetItems();
        if (serviceResponse.ResponseObject != null)
        {
            configdata.ResourceTypes = serviceResponse.ResponseObject!;
        }

        // ResourceUnitDepts
        serviceResponse = _resourceUnitDeptService.GetItems();
        if (serviceResponse.ResponseObject != null)
        {
            configdata.ResourceUnitDepts = serviceResponse.ResponseObject!;
        }

        // CustomComponents
        serviceResponse = _customComponentService.GetItems();
        if (serviceResponse.ResponseObject != null)
        {
            configdata.CustomComponents = serviceResponse.ResponseObject!;
        }

        //GeneratedNames
        serviceResponse = _generatedNamesService.GetItems();
        if (serviceResponse.ResponseObject != null)
        {
            configdata.GeneratedNames = serviceResponse.ResponseObject!;
        }

        //AdminLogs
        serviceResponse = _adminLogService.GetItems();
        if (serviceResponse.ResponseObject != null)
        {
            configdata.AdminLogs = serviceResponse.ResponseObject;
        }

        // Get the current settings
        configdata.DismissedAlerts = _siteConfiguration.DismissedAlerts;
        configdata.DuplicateNamesAllowed = _siteConfiguration.DuplicateNamesAllowed;
        configdata.ConnectivityCheckEnabled = _siteConfiguration.ConnectivityCheckEnabled;
        configdata.GenerationWebhook = _siteConfiguration.GenerationWebhook;

        // Get the security settings
        if (includeadmin)
        {
            configdata.SALTKey = _siteConfiguration.SaltKey;
            configdata.AdminPassword = _siteConfiguration.AdminPassword;
            configdata.APIKey = _siteConfiguration.ApiKey;
            //IdentityHeaderName
            configdata.IdentityHeaderName = _siteConfiguration.IdentityHeaderName;
            //AdminUsers
            serviceResponse = _adminUserService.GetItems();
            if (serviceResponse.ResponseObject != null)
            {
                configdata.AdminUsers = serviceResponse.ResponseObject!;
            }

            // ResourceTypeEditing
            configdata.ResourceTypeEditingAllowed = _siteConfiguration.ResourceTypeEditingAllowed;
        }

        serviceResponse.ResponseObject = configdata;
        serviceResponse.Success = true;

        return serviceResponse;
    }

    public async Task<ServiceResponse> PostConfig(ConfigurationData configData)
    {
        ServiceResponse serviceResponse = new();

        // Write all the configurations
        _resourceComponentService.PostConfig(configData.ResourceComponents);
        _resourceDelimiterService.PostConfig(configData.ResourceDelimiters);
        _resourceEnvironmentService.PostConfig(configData.ResourceEnvironments);
        _resourceFunctionService.PostConfig(configData.ResourceFunctions);
        _resourceLocationService.PostConfig(configData.ResourceLocations);
        _resourceOrgService.PostConfig(configData.ResourceOrgs);
        _resourceProjAppSvcService.PostConfig(configData.ResourceProjAppSvcs);
        _resourceTypeService.PostConfig(configData.ResourceTypes);
        _resourceUnitDeptService.PostConfig(configData.ResourceUnitDepts);
        _customComponentService.PostConfig(configData.CustomComponents);
        _generatedNamesService.PostConfig(configData.GeneratedNames);
        _adminUserService.PostConfig(configData.AdminUsers);
        
        if (configData.AdminLogs != null)
        {
            _adminLogService.PostConfig(configData.AdminLogs);
        }

        _siteConfiguration.DismissedAlerts = configData.DismissedAlerts;
        _siteConfiguration.DuplicateNamesAllowed = configData.DuplicateNamesAllowed;
        _siteConfiguration.ConnectivityCheckEnabled = configData.ConnectivityCheckEnabled;

        // Set the admin settings, if they are included in the import
        if (configData.SALTKey != null)
        {
            _siteConfiguration.SaltKey = configData.SALTKey;
        }

        if (configData.AdminPassword != null)
        {
            _siteConfiguration.AdminPassword = configData.AdminPassword;
        }

        if (configData.APIKey != null)
        {
            _siteConfiguration.ApiKey = configData.APIKey;
        }

        if (configData.IdentityHeaderName != null)
        {
            _siteConfiguration.IdentityHeaderName = configData.IdentityHeaderName;
        }

        if (configData.ResourceTypeEditingAllowed != null)
        {
            _siteConfiguration.ResourceTypeEditingAllowed = configData.ResourceTypeEditingAllowed;
        }

        var jsonWriteOptions = new JsonSerializerOptions
        {
            WriteIndented = true
        };
        jsonWriteOptions.Converters.Add(new JsonStringEnumConverter());

        var newJson = JsonSerializer.Serialize(_siteConfiguration, jsonWriteOptions);

        var appSettingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings/appsettings.json");
        await File.WriteAllTextAsync(appSettingsPath, newJson);

        _cacheHelper.ClearAllCache();
        serviceResponse.Success = true;

        return serviceResponse;
    }
}