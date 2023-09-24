using AzureNamingTool.Data.SourceRepository;
using AzureNamingTool.Helpers;
using AzureNamingTool.Models;
using AzureNamingTool.Services;

namespace AzureNamingTool;

public static class ServicesExtensions
{
    public static void RegisterApplicationDependencies(this IServiceCollection services)
    {
        services.AddSingleton<StateContainer>();
        services.AddSingleton<CacheHelper>();
        services.AddTransient<ConfigurationHelper>();
        services.AddTransient<FileSystemHelper>();
        services.AddTransient<GeneralHelper>();
        services.AddTransient<IdentityHelper>();
        services.AddTransient<LogHelper>();
        services.AddTransient<ServicesHelper>();
        services.AddTransient<ValidationHelper>();
        services.AddTransient<EncryptionHelper>();

        services.AddTransient<FileReader>();
        services.AddTransient<FileWriter>();

        services.AddSingleton<RepositoryFactory>();
        
        services.AddTransient<AdminLogService>();
        services.AddTransient<AdminService>();
        services.AddTransient<AdminUserService>();
        services.AddTransient<CustomComponentService>();
        services.AddTransient<GeneratedNamesService>();
        services.AddTransient<ImportExportService>();
        services.AddTransient<PolicyService>();
        services.AddTransient<ResourceComponentService>();
        services.AddTransient<ResourceDelimiterService>();
        services.AddTransient<ResourceEnvironmentService>();
        services.AddTransient<ResourceFunctionService>();
        services.AddTransient<ResourceLocationService>();
        services.AddTransient<ResourceNamingRequestService>();
        services.AddTransient<ResourceOrgService>();
        services.AddTransient<ResourceProjAppSvcService>();
        services.AddTransient<ResourceTypeService>();
        services.AddTransient<ResourceUnitDeptService>();

        services.AddTransient<ResourceTypeUpdater>();
        services.AddTransient<ResourceComponentDeleter>();
        services.AddTransient<HttpContentDownloader>();
        services.AddTransient<GithubConnectivityChecker>();

        services.AddTransient<SiteConfiguration>(provider =>
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("settings/appsettings.json")
                .Build();

            var siteConfiguration = new SiteConfiguration
            {
                AdminPassword = config["AdminPassword"],
                ApiKey = config["ApiKey"],
                AppTheme = config["AppTheme"],
                ConnectivityCheckEnabled = Convert.ToBoolean(config["ConnectivityCheckEnabled"]),
                DevMode = Convert.ToBoolean(config["DevMode"]),
                DismissedAlerts = config["DismissedAlerts"],
                DuplicateNamesAllowed = config["DuplicateNamesAllowed"],
                GenerationWebhook = config["GenerationWebhook"],
                IdentityHeaderName = config["IdentityHeaderName"],
                ResourceTypeEditingAllowed = config["ResourceTypeEditingAllowed"],
                SaltKey = config["SaltKey"]
            };

            return siteConfiguration;
        });
    }
}