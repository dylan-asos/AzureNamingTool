using System.Collections;
using System.Reflection;
using System.Runtime.Caching;
using System.Text.Json;
using System.Text.Json.Serialization;
using AzureNamingTool.Models;
using AzureNamingTool.Services;

namespace AzureNamingTool.Helpers;

public class ConfigurationHelper
{
    private readonly AdminLogService _adminLogService;
    private readonly CacheHelper _cacheHelper;
    private readonly GithubConnectivityChecker _connectivityChecker;
    private readonly FileSystemHelper _fileSystemHelper;
    private readonly GeneralHelper _generalHelper;
    private readonly HttpContentDownloader _httpContentDownloader;
    private readonly SiteConfiguration _siteConfiguration;

    public ConfigurationHelper(
        FileSystemHelper fileSystemHelper,
        CacheHelper cacheHelper,
        GeneralHelper generalHelper,
        AdminLogService adminLogService,
        HttpContentDownloader httpContentDownloader,
        GithubConnectivityChecker connectivityChecker,
        SiteConfiguration siteConfiguration
    )
    {
        _fileSystemHelper = fileSystemHelper;
        _cacheHelper = cacheHelper;
        _generalHelper = generalHelper;
        _adminLogService = adminLogService;
        _httpContentDownloader = httpContentDownloader;
        _connectivityChecker = connectivityChecker;
        _siteConfiguration = siteConfiguration;
    }


    public string GetAppSetting(string key, bool decrypt = false)
    {
        string value;

        // Check if the data is cached
        var items = _cacheHelper.GetCacheObject(key);
        if (items == null)
        {
            // Check if the app setting is already set
            if (_siteConfiguration.GetType().GetProperty(key) != null)
            {
                value = _siteConfiguration!.GetType()!.GetProperty(key)!.GetValue(_siteConfiguration, null)!
                    .ToString()!;

                // Verify the value is encrypted, and should be decrypted
                if (decrypt && !string.IsNullOrEmpty(value) && _generalHelper.IsBase64Encoded(value))
                {
                    value = _generalHelper.DecryptString(value, _siteConfiguration.SaltKey!);
                }

                // Set the result to cache
                _cacheHelper.SetCacheObject(key, value!);
            }
            else
            {
                // Create a new configuration object and get the default for the property
                SiteConfiguration newconfig = new();
                value = newconfig!.GetType()!.GetProperty(key)!.GetValue(newconfig, null)!.ToString()!;

                // Set the result to the app settings
                SetAppSetting(key, value, decrypt);

                // Set the result to cache
                _cacheHelper.SetCacheObject(key, value);
            }
        }
        else
        {
            value = items.ToString()!;
        }

        return value;
    }

    public void SetAppSetting(string key, string value, bool encrypt = false)
    {
        var valueoriginal = value;
        if (encrypt)
        {
            value = _generalHelper.EncryptString(value, _siteConfiguration.SaltKey!);
        }

        var type = _siteConfiguration.GetType();
        var propertyInfo = type.GetProperty(key)!;
        propertyInfo.SetValue(_siteConfiguration, value, null);
        UpdateSettings(_siteConfiguration);
        // Save the original value to the cache
        _cacheHelper.SetCacheObject(key, valueoriginal);
    }

    public void VerifyConfiguration()
    {
        // Get all the files in the repository folder
        DirectoryInfo repositoryDir = new("repository");
        foreach (var file in repositoryDir.GetFiles())
        {
            // Check if the file exists in the settings folder
            if (!File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings/" + file.Name)))
            {
                // Copy the repository file to the settings folder
                file.CopyTo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings/" + file.Name));
            }
        }

        // Migrate old data to new files, if needed
        // Check if the admin log file exists in the settings folder and the adminmessages does not
        if (File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings/adminlog.json")) &&
            !File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings/adminlogmessages.json")))
        {
            // Migrate the data
            _fileSystemHelper.MigrateDataToFile("adminlog.json", "settings/", FileNames.AdminLogMessage,
                "settings/", true);
        }
    }

    public void VerifySecurity(StateContainer state)
    {
        if (!state.Verified)
        {
            if (string.IsNullOrEmpty(_siteConfiguration.SaltKey))
            {
                // Create a new SALT key 
                const string chars = "abcdefghijklmnopqrstuvwxyz0123456789";
                Random random = new();
                var salt = new string(Enumerable.Repeat(chars, 16)
                    .Select(s => s[random.Next(s.Length)]).ToArray());

                _siteConfiguration.SaltKey = salt;
                _siteConfiguration.ApiKey = _generalHelper.EncryptString(_siteConfiguration.ApiKey!, salt);

                if (!string.IsNullOrEmpty(_siteConfiguration.AdminPassword))
                {
                    _siteConfiguration.AdminPassword = _generalHelper.EncryptString(_siteConfiguration.AdminPassword,
                        _siteConfiguration.SaltKey);
                    state.Password = true;
                }
                else
                {
                    state.Password = false;
                }
            }

            if (!string.IsNullOrEmpty(_siteConfiguration.AdminPassword))
            {
                state.Password = true;
            }
            else
            {
                state.Password = false;
            }

            UpdateSettings(_siteConfiguration);
        }

        state.SetVerified(true);

        // Set the site theme
        state.SetAppTheme(_siteConfiguration.AppTheme!);
    }


    public void UpdateSettings(SiteConfiguration config)
    {
        // Clear the cache
        ObjectCache memoryCache = MemoryCache.Default;
        var cacheKeys = memoryCache.Select(kvp => kvp.Key).ToList();
        foreach (var cacheKey in cacheKeys)
        {
            memoryCache.Remove(cacheKey);
        }

        var jsonWriteOptions = new JsonSerializerOptions
        {
            WriteIndented = true
        };
        jsonWriteOptions.Converters.Add(new JsonStringEnumConverter());

        var newJson = JsonSerializer.Serialize(config, jsonWriteOptions);

        _fileSystemHelper.WriteFile("appsettings.json", newJson);
    }

    private async Task<string> GetOfficialConfigurationFileVersionData()
    {
        var versionData =
            await _httpContentDownloader.DownloadString(
                "https://raw.githubusercontent.com/mspnp/AzureNamingTool/main/src/configurationfileversions.json");

        return versionData;
    }

    private string GetCurrentConfigFileVersionData()
    {
        var versiondatajson = string.Empty;

        versiondatajson = _fileSystemHelper.ReadFile("configurationfileversions.json");
        // Check if the user has any version data. This value will be '[]' if not.
        if (versiondatajson == "[]")
        {
            // Create new version data with default values in /settings file
            ConfigurationFileVersionData? versiondata = new();
            _fileSystemHelper.WriteFile("configurationfileversions.json",
                JsonSerializer.Serialize(versiondata));
            versiondatajson = JsonSerializer.Serialize(versiondata);
        }
        
        return versiondatajson;
    }

    public async Task<List<string>> VerifyConfigurationFileVersionData()
    {
        List<string> versiondata = new();

        // Get the official version from GitHub
        ConfigurationFileVersionData? officialversiondata = new();
        var officialdatajson = await GetOfficialConfigurationFileVersionData();

        // Get the current version
        ConfigurationFileVersionData? currentversiondata = new();
        var currentdatajson = GetCurrentConfigFileVersionData();

        // Determine if the version data is different
        if (officialdatajson != null && currentdatajson != null)
        {
            officialversiondata = JsonSerializer.Deserialize<ConfigurationFileVersionData>(officialdatajson);
            currentversiondata = JsonSerializer.Deserialize<ConfigurationFileVersionData>(currentdatajson);

            if (officialversiondata != null && currentversiondata != null)
            {
                // Compare the versions
                // Resource Types
                if (officialversiondata.resourcetypes != currentversiondata.resourcetypes)
                {
                    versiondata.Add(
                        "<h5>Resource Types</h5><hr /><div>The Resource Types Configuration is out of date!<br /><br />It is recommended that you refresh your resource types to the latest configuration.<br /><br /><strong>To Refresh:</strong><ul><li>Expand the <strong>Types</strong> section</li><li>Expand the <strong>Configuration</strong> section</li><li>Select the <strong>Refresh</strong> option</li></ul></div><br />");
                }

                // Resource Locations
                if (officialversiondata.resourcelocations != currentversiondata.resourcelocations)
                {
                    versiondata.Add(
                        "<h5>Resource Locations</h5><hr /><div>The Resource Locations Configuration is out of date!<br /><br />It is recommended that you refresh your resource locations to the latest configuration.<br /><br /><strong>To Refresh:</strong><ul><li>Expand the <strong>Locations</strong> section</li><li>Expand the <strong>Configuration</strong> section</li><li>Select the <strong>Refresh</strong> option</li></ul></div><br />");
                }
            }
        }

        return versiondata;
    }

    public async Task UpdateConfigurationFileVersion(string fileName)
    {
        if (await _connectivityChecker.VerifyConnectivity())
        {
            // Get the official version from GitHub
            ConfigurationFileVersionData? officialversiondata = new();
            var officialdatajson = await GetOfficialConfigurationFileVersionData();

            // Get the current version
            ConfigurationFileVersionData? currentversiondata = new();
            var currentdatajson = GetCurrentConfigFileVersionData();

            // Determine if the version data is different
            if (officialdatajson != null && currentdatajson != null)
            {
                officialversiondata = JsonSerializer.Deserialize<ConfigurationFileVersionData>(officialdatajson);
                currentversiondata = JsonSerializer.Deserialize<ConfigurationFileVersionData>(currentdatajson);

                if (officialversiondata != null && currentversiondata != null)
                {
                    switch (fileName)
                    {
                        case "resourcetypes":
                            currentversiondata.resourcetypes = officialversiondata.resourcetypes;
                            break;
                        case "resourcelocations":
                            currentversiondata.resourcelocations = officialversiondata.resourcelocations;
                            break;
                    }

                    //  Update the current configuration file version data
                    _fileSystemHelper.WriteFile("configurationfileversions.json",
                        JsonSerializer.Serialize(currentversiondata));
                }
            }
        }
    }

    public bool ResetSiteConfiguration()
    {
        var result = false;

        // Get all the files in the repository folder
        DirectoryInfo repositoryDir = new("repository");
        // Filter out the appsettings.json to retain admin credentials
        string[] protectedfilenames = {FileNames.AdminUser, "appsettings.json"};
        foreach (var file in repositoryDir.GetFiles())
        {
            //Only copy non-admin files
            if (!protectedfilenames.Contains(file.Name.ToLower()))
            {
                // Copy the repository file to the settings folder
                file.CopyTo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings/" + file.Name), true);
            }
        }

        // Clear the cache
        ObjectCache memoryCache = MemoryCache.Default;
        var cacheKeys = memoryCache.Select(kvp => kvp.Key).ToList();
        foreach (var cacheKey in cacheKeys)
        {
            memoryCache.Remove(cacheKey);
        }

        result = true;

        return result;
    }

    public async Task<string?> GetToolVersion()
    {
        var versiondata = string.Empty;
        versiondata = await GetProgramSetting("toolVersion");
        return versiondata;
    }

    public string GetVersionAlert(bool forceDisplay = false)
    {
        var alert = "";

        VersionAlert versionalert = new();
        var dismissed = false;
        var appversion = Assembly.GetEntryAssembly()!.GetCustomAttribute<AssemblyInformationalVersionAttribute>()!
            .InformationalVersion;

        // Check if version alert has been dismissed
        var dismissedAlertSetting = GetAppSetting("DismissedAlerts");
        var dismissedAlerts = dismissedAlertSetting.Split(',');

        if (dismissedAlerts != null)
        {
            if (dismissedAlerts.Contains(appversion))
            {
                dismissed = true;
            }
        }

        if (!dismissed || forceDisplay)
        {
            // Check if the data is cached
            var cacheddata = _cacheHelper.GetCacheObject("versionalert-" + appversion);
            if (cacheddata == null)
            {
                // Get the alert 
                var data = _fileSystemHelper.ReadFile("versionalerts.json", "");
                if (data != null)
                {
                    var items = new List<VersionAlert>();
                    var options = new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                        PropertyNameCaseInsensitive = true
                    };
                    items = JsonSerializer.Deserialize<List<VersionAlert>>(data, options)!.ToList();
                    versionalert = items.FirstOrDefault(x => x.Version == appversion)!;

                    if (versionalert != null)
                    {
                        alert = versionalert.Alert;
                        // Set the result to cache
                        _cacheHelper.SetCacheObject("versionalert-" + appversion, versionalert.Alert);
                    }
                }
            }
            else
            {
                alert = (string) cacheddata;
            }
        }

        return alert;
    }

    public void DismissVersionAlert()
    {
        var appVersion = Assembly
            .GetEntryAssembly()!
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()!
            .InformationalVersion;

        List<string> dismissedAlerts
            = new(GetAppSetting("DismissedAlerts").Split(','));

        if (!dismissedAlerts.Contains(appVersion))
        {
            if (string.IsNullOrEmpty(string.Join(",", dismissedAlerts)))
            {
                dismissedAlerts.Clear();
            }

            dismissedAlerts.Add(appVersion);
        }

        SetAppSetting("DismissedAlerts", string.Join(",", dismissedAlerts));
    }

    public bool VerifyDuplicateNamesAllowed()
    {
        bool result;

        // Check if the data is cached
        var cacheddata = _cacheHelper.GetCacheObject("duplicatenamesallowed");
        if (cacheddata == null)
        {
            // Check if version alert has been dismissed
            var allowed = GetAppSetting("DuplicateNamesAllowed");
            result = Convert.ToBoolean(allowed);
            // Set the result to cache
            _cacheHelper.SetCacheObject("duplicatenamesallowed", result);
        }
        else
        {
            result = Convert.ToBoolean(cacheddata);
        }

        return result;
    }

    public async Task<bool> PostToGenerationWebhook(string URL, GeneratedName generatedName)
    {
        var result = false;

        HttpClient httpClient = new()
        {
            BaseAddress = new Uri(URL)
        };
        var response = await httpClient.PostAsJsonAsync("", generatedName);
        if (response.IsSuccessStatusCode)
        {
            result = true;
            _adminLogService.PostItem(new AdminLogMessage
            {
                Title = "INFORMATION",
                Message = "Generated Name (" + generatedName.ResourceName + ") successfully posted to webhook!"
            });
        }
        else
        {
            _adminLogService.PostItem(new AdminLogMessage
            {
                Title = "INFORMATION",
                Message = "Generated Name (" + generatedName.ResourceName + ") not successfully posted to webhook! " +
                          response.ReasonPhrase
            });
        }

        return result;
    }

    public async Task<string> GetProgramSetting(string programSetting)
    {
        var result = string.Empty;

        var data = (string) _cacheHelper.GetCacheObject(programSetting)!;
        if (string.IsNullOrEmpty(data))
        {
            var response =
                await _httpContentDownloader.DownloadString(
                    "https://raw.githubusercontent.com/mspnp/AzureNamingTool/main/src/programsettings.json");
            var setting = JsonDocument.Parse(response);
            result = setting.RootElement.GetProperty(programSetting).ToString();
            _cacheHelper.SetCacheObject(programSetting, result);
        }
        else
        {
            result = data;
        }

        return result;
    }


    public static List<KeyValuePair<string, string>> GetEnvironmentVariables()
    {
        var entries = Environment.GetEnvironmentVariables().Cast<DictionaryEntry>()
            .Select(x => KeyValuePair.Create((string) x.Key, (string) x.Value!));
        var sortedEntries = entries.OrderBy(x => x.Key);
        var result = sortedEntries.ToList();

        return result;
    }
}