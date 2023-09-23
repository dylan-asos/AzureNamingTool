using AzureNamingTool.Models;
using AzureNamingTool.Services;
using System.Runtime.Caching;
using System.Text;

namespace AzureNamingTool.Helpers
{
    public class CacheHelper
    {
        public object? GetCacheObject(string cacheKey)
        {
            ObjectCache memoryCache = MemoryCache.Default;
            var encodedCache = memoryCache.Get(cacheKey);
            return encodedCache;
        }

        public void SetCacheObject(string cacheKey, object cacheData)
        {
            ObjectCache memoryCache = MemoryCache.Default;
            var cacheItemPolicy = new CacheItemPolicy
            {
                AbsoluteExpiration = DateTimeOffset.Now.AddSeconds(600.0),

            };
            memoryCache.Set(cacheKey, cacheData, cacheItemPolicy);
        }

        public void InvalidateCacheObject(string cacheKey)
        {
            ObjectCache memoryCache = MemoryCache.Default;
            memoryCache.Remove(cacheKey);
        }


        public string GetAllCacheData()
        {
            StringBuilder data = new();
            try
            {
                ObjectCache memoryCache = MemoryCache.Default;
                var cacheKeys = memoryCache.Select(kvp => kvp.Key).ToList();
                foreach (var key in cacheKeys.OrderBy(x => x))
                {
                    data.Append("<p><strong>" + key + "</strong></p><div class=\"alert alert-secondary\" style=\"word-wrap:break-word;\">" + MemoryCache.Default[key].ToString() + "</div>");
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
            ObjectCache memoryCache = MemoryCache.Default;
            List<string> cacheKeys = memoryCache.Select(kvp => kvp.Key).ToList();
            foreach (string cacheKey in cacheKeys)
            {
                memoryCache.Remove(cacheKey);
            }
        }
    }
}
