using DistributedWebCrawler.Core.Interfaces;
using Microsoft.Extensions.Caching.Distributed;

namespace DistributedWebCrawler.Extensions.Redis
{
    public class RedisKeyValueStore : IKeyValueStore
    {
        private readonly IDistributedCache _cache;

        public RedisKeyValueStore(IDistributedCache cache)
        {
            _cache = cache;
        }

        public Task PutAsync(string key, string value, CancellationToken cancellationToken, TimeSpan? expireAfter)
        {
            if (expireAfter == null)
            {
                return _cache.SetStringAsync(key, value, cancellationToken);
            }
            else
            {
                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = expireAfter
                };

                return _cache.SetStringAsync(key, value, options, cancellationToken);
            }
        }

        public Task<string?> GetAsync(string key, CancellationToken cancellationToken)
        {
            
            return _cache.GetStringAsync(key, cancellationToken);
        }

        public Task RemoveAsync(string key, CancellationToken cancellationToken)
        {
            return _cache.RemoveAsync(key, cancellationToken);
        }
    }
}
