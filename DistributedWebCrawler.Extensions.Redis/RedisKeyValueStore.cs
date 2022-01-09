using DistributedWebCrawler.Core.Interfaces;

namespace DistributedWebCrawler.Extensions.Redis
{
    public class RedisKeyValueStore : IKeyValueStore
    {
        private readonly IConnectionMultiplexerPool _connectionMultiplexerPool;
        private readonly ISerializer _serializer;

        public RedisKeyValueStore(IConnectionMultiplexerPool connectionMultiplexerPool, ISerializer serializer)
        {
            _connectionMultiplexerPool = connectionMultiplexerPool;
            _serializer = serializer;
        }

        public async Task PutAsync(string key, string value, TimeSpan? expireAfter)
        {
            var database = await _connectionMultiplexerPool.GetDatabaseAsync().ConfigureAwait(false);
            database.StringSetAsync(key, value, expireAfter);
        }

        public async Task<string?> GetAsync(string key)
        {
            var database = await _connectionMultiplexerPool.GetDatabaseAsync().ConfigureAwait(false);
            var result = await database.StringGetAsync(key).ConfigureAwait(false);
            return result;
        }

        public async Task RemoveAsync(string key)
        {
            var database = await _connectionMultiplexerPool.GetDatabaseAsync().ConfigureAwait(false);
            database.KeyDeleteAsync(key);
        }

        public async Task PutAsync<TData>(string key, TData value, TimeSpan? expireAfter = null) 
            where TData : notnull
        {
            var bytes = _serializer.Serialize(value);

            var database = await _connectionMultiplexerPool.GetDatabaseAsync().ConfigureAwait(false);

            database.StringSetAsync(key, bytes, expireAfter);
        }

        public async Task<TData?> GetAsync<TData>(string key)
        {
            var database = await _connectionMultiplexerPool.GetDatabaseAsync().ConfigureAwait(false);
            var bytes = await database.StringGetAsync(key).ConfigureAwait(false);

            if (!bytes.HasValue)
            {
                return default;
            }

            return _serializer.Deserialize<TData?>(bytes.ToString());
        }
    }
}