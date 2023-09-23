using AzureNamingTool.Helpers;
using AzureNamingTool.Models;

namespace AzureNamingTool.Services;

public class AdminService
{
    private readonly SiteConfiguration _config;
    private readonly ConfigurationHelper _configurationHelper;
    private readonly GeneralHelper _generalHelper;
    private readonly ValidationHelper _validationHelper;

    public AdminService(
        ValidationHelper validationHelper, 
        ConfigurationHelper configurationHelper, 
        GeneralHelper generalHelper, 
        SiteConfiguration config)
    {
        _validationHelper = validationHelper;
        _configurationHelper = configurationHelper;
        _generalHelper = generalHelper;
        _config = config;
    }

    public ServiceResponse UpdatePassword(string password)
    {
        ServiceResponse serviceResponse = new();

        if (_validationHelper.ValidatePassword(password))
        {
            _config.AdminPassword = _generalHelper.EncryptString(password, _config.SaltKey!);
            _configurationHelper.UpdateSettings(_config);
            serviceResponse.Success = true;
        }
        else
        {
            serviceResponse.Success = false;
            serviceResponse.ResponseObject = "The password does not meet the security requirements.";
        }

        return serviceResponse;
    }

    public ServiceResponse GenerateApiKey()
    {
        // Set the new api key
        var guid = Guid.NewGuid();
        _config.ApiKey = _generalHelper.EncryptString(guid.ToString(), _config.SaltKey!);
        _configurationHelper.UpdateSettings(_config);
        ServiceResponse serviceResponse = new()
        {
            ResponseObject = guid.ToString(),
            Success = true
        };

        return serviceResponse;
    }

    public ServiceResponse UpdateApiKey(string apikey)
    {
        _config.ApiKey = _generalHelper.EncryptString(apikey, _config.SaltKey!);
        _configurationHelper.UpdateSettings(_config);

        ServiceResponse serviceResponse = new()
        {
            ResponseObject = apikey,
            Success = true
        };

        return serviceResponse;
    }

    public ServiceResponse UpdateIdentityHeaderName(string identityHeaderName)
    {
        _config.IdentityHeaderName = _generalHelper.EncryptString(identityHeaderName, _config.SaltKey!);
        _configurationHelper.UpdateSettings(_config);

        ServiceResponse serviceResponse = new()
        {
            ResponseObject = identityHeaderName,
            Success = true
        };
        
        return serviceResponse;
    }
}