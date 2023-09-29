using AzureNamingTool.Helpers;
using AzureNamingTool.Models;

namespace AzureNamingTool.Services;

public class AdminService
{
    private readonly SiteConfiguration _config;
    private readonly ConfigurationHelper _configurationHelper;
    private readonly EncryptionHelper _encryptionHelper;
    private readonly ValidationHelper _validationHelper;

    public AdminService(
        ValidationHelper validationHelper,
        ConfigurationHelper configurationHelper,
        SiteConfiguration config,
        EncryptionHelper encryptionHelper)
    {
        _validationHelper = validationHelper;
        _configurationHelper = configurationHelper;
        _config = config;
        _encryptionHelper = encryptionHelper;
    }

    public ServiceResponse UpdatePassword(string password)
    {
        ServiceResponse serviceResponse = new();

        if (_validationHelper.ValidatePassword(password))
        {
            _config.AdminPassword = _encryptionHelper.EncryptString(password, _config.SaltKey!);
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
        _config.ApiKey = _encryptionHelper.EncryptString(guid.ToString(), _config.SaltKey!);
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
        _config.ApiKey = _encryptionHelper.EncryptString(apikey, _config.SaltKey!);
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
        _config.IdentityHeaderName = _encryptionHelper.EncryptString(identityHeaderName, _config.SaltKey!);
        _configurationHelper.UpdateSettings(_config);

        ServiceResponse serviceResponse = new()
        {
            ResponseObject = identityHeaderName,
            Success = true
        };

        return serviceResponse;
    }
}