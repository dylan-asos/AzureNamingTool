using System.Net;
using System.Net.NetworkInformation;
using AzureNamingTool.Models;

namespace AzureNamingTool.Helpers;

public class GithubConnectivityChecker
{
    private readonly CacheHelper _cacheHelper;
    private readonly ILogger<GithubConnectivityChecker> _logger;
    private readonly SiteConfiguration _siteConfiguration;

    public GithubConnectivityChecker(
        CacheHelper cacheHelper, 
        ILogger<GithubConnectivityChecker> logger, 
        SiteConfiguration siteConfiguration)
    {
        _cacheHelper = cacheHelper;
        _logger = logger;
        _siteConfiguration = siteConfiguration;
    }
    
    public async Task<bool> VerifyConnectivity()
    {
        var pingSuccessful = false;
        var result = false;

        // Check if the data is cached
        var items = _cacheHelper.GetCacheObject("isconnected");
        
        if (items == null)
        {
            // Check if the connectivity check is enabled
            if (_siteConfiguration.ConnectivityCheckEnabled)
            {
                // Atempt to ping a url first
                Ping ping = new();
                const string host = "github.com";
                var buffer = new byte[32];
                const int timeout = 1000;
                PingOptions pingOptions = new();
                try
                {
                    var reply = ping.Send(host, timeout, buffer, pingOptions);
                    if (reply.Status == IPStatus.Success)
                    {
                        pingSuccessful = true;
                        result = true;
                    }
                }
                catch (Exception)
                {
                    // Catch this exception but continue to try a web request instead
                }

                // If ping is not successful, attempt to download a file
                if (!pingSuccessful)
                {
                    // Attempt to download a file
                    var client = new HttpClient(new HttpClientHandler {UseDefaultCredentials = true});
                    using var response =
                        await client.GetAsync(
                            "https://github.com/mspnp/AzureNamingTool/blob/main/src/connectiontest.png");
                    
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        result = true;
                    }
                    else
                    {
                        _logger.LogError("Connectivity Check Failed: {ResponseReasonPhrase}", response.ReasonPhrase);
                    }
                }
            }
            else
            {
                result = true;
            }
        }
        else
        {
            result = (bool) items;
        }

        // Set the result to cache
        _cacheHelper.SetCacheObject("isconnected", result);
        
        return result;
    }
}