using System.Runtime.Caching;
using System.Text;

namespace AzureNamingTool.Helpers;

public class CacheHelper
{
    private readonly MemoryCache _memoryCache;

    public CacheHelper()
    {
        _memoryCache = MemoryCache.Default;
    }


    public object? GetCacheObject(string cacheKey)
    {
        var encodedCache = _memoryCache.Get(cacheKey);
        return encodedCache;
    }

    public void SetCacheObject(string cacheKey, object cacheData)
    {
        var cacheItemPolicy = new CacheItemPolicy
        {
            AbsoluteExpiration = DateTimeOffset.Now.AddSeconds(600.0)
        };
        _memoryCache.Set(cacheKey, cacheData, cacheItemPolicy);
    }

    public void InvalidateCacheObject(string cacheKey)
    {
        _memoryCache.Remove(cacheKey);
    }

    public string GetAllCacheData()
    {
        StringBuilder data = new();
        try
        {
            var cacheKeys = _memoryCache.Select(kvp => kvp.Key).ToList();
            foreach (var key in cacheKeys.OrderBy(x => x))
            {
                data.Append("<p><strong>" + key +
                            "</strong></p><div class=\"alert alert-secondary\" style=\"word-wrap:break-word;\">" +
                            MemoryCache.Default[key] + "</div>");
            }
        }
        catch (Exception ex)
        {
            data.Append("<p><strong>No data currently cached.</strong></p>");
        }

        return data.ToString();
    }

    public void ClearAllCache()
    {
        var cacheKeys = _memoryCache.Select(kvp => kvp.Key).ToList();
        foreach (var cacheKey in cacheKeys)
        {
            _memoryCache.Remove(cacheKey);
        }
    }
}