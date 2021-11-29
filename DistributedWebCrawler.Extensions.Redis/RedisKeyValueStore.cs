using DistributedWebCrawler.Core.Interfaces;
using Microsoft.Extensions.Caching.Distributed;

namespace DistributedWebCrawler.Extensions.Redis
{
    public class RedisKeyValueStore : IKeyValueStore
    {
        private readonly IDistributedCache _cache;
        private readonly ISerializer _serializer;

        public RedisKeyValueStore(IDistributedCache cache, ISerializer serializer)
        {
            _cache = cache;
            _serializer = serializer;
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

        public Task PutAsync<TData>(string key, TData value, CancellationToken cancellationToken, TimeSpan? expireAfter = null) 
            where TData : notnull
        {
            var bytes = _serializer.Serialize(value);

            if (expireAfter == null)
            {
                return _cache.SetAsync(key, bytes, cancellationToken);
            }
            else
            {
                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = expireAfter
                };

                return _cache.SetAsync(key, bytes, options, cancellationToken);
            }
        }

        public async Task<TData?> GetAsync<TData>(string key, CancellationToken cancellationToken)
        {
            var bytes = await _cache.GetAsync(key, cancellationToken).ConfigureAwait(false);

            if (bytes == null)
            {
                return default;
            }

            return _serializer.Deserialize<TData?>(bytes);
        }
    }
}
