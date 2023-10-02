namespace AzureNamingTool.Models
{
    public class SiteConfiguration
    {
        public string? SaltKey { get; set; }
        public string? AdminPassword { get; set; }
        public string? ApiKey { get; set; }
        public string? AppTheme { get; set; }
        
        public bool? DevMode { get; set; } = false;
        public string? DismissedAlerts { get; set; }
        public string? DuplicateNamesAllowed { get; set; }
        public string? GenerationWebhook { get; set; }
        
        public bool ConnectivityCheckEnabled { get; set; }
        public string? IdentityHeaderName { get; set; }
        public string? ResourceTypeEditingAllowed { get; set; }
        public string? RepositoryType { get; set; } = "File";
        
        public string? StorageAccountName { get; set; } 
    }
}
