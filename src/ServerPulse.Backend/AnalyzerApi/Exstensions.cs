using ServerMonitorApi.Services;
using Shared;

namespace AnalyzerApi
{
    public static class Exstensions
    {
        public static async Task<T?> GetInCacheAsync<T>(this ICacheService cacheService, string key) where T : class
        {
            var json = await cacheService.GetValueAsync(key);
            if (string.IsNullOrEmpty(json))
            {
                return null;
            }
            if (json.TryToDeserialize(out T response))
            {
                return response;
            }
            return null;
        }
    }
}
