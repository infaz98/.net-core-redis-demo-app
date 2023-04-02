using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace RedisNetCore.Handlers
{
    public class RedisHandler
    {
        private TimeSpan _unusedExpireTime;
        private TimeSpan _absoluteExpireTime;
        private readonly IDistributedCache _distributedCache;

        public RedisHandler(IDistributedCache distributedCache, int absoluteExpireTime, int unusedExpireTime)
        {   
            _distributedCache = distributedCache;
            _absoluteExpireTime = TimeSpan.FromSeconds(absoluteExpireTime);
            _unusedExpireTime = TimeSpan.FromSeconds(unusedExpireTime);
        }

        public async Task SaveRecordAsync<T>(IDistributedCache cache, string recordId, T data)
        {
            try
            {
                var options = new DistributedCacheEntryOptions();

                options.AbsoluteExpirationRelativeToNow = _absoluteExpireTime;
                options.SlidingExpiration = _unusedExpireTime;

                var jsonData = JsonSerializer.Serialize(data);
                await cache.SetStringAsync(recordId, jsonData, options);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<T> GetRecordAsync<T>(IDistributedCache cache, string recordId)
        {
            try
            {
                var jsonData = await cache.GetStringAsync(recordId);

                if (jsonData is null)
                {
                    return default(T);
                }
                return JsonSerializer.Deserialize<T>(jsonData);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task DeleteRecordAsync<T>(IDistributedCache cache, string recordId)
        {
            try
            {
                await cache.RemoveAsync(recordId);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
