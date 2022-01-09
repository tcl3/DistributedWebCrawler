using StackExchange.Redis;

namespace DistributedWebCrawler.Extensions.Redis
{
    public class ConnectionMultiplexerPool : IConnectionMultiplexerPool
    {
        private readonly IConnectionMultiplexer[] _pool;
        private readonly ConfigurationOptions _redisConfigurationOptions;
        private readonly SemaphoreSlim _lock;

        public ConnectionMultiplexerPool(ConfigurationOptions redisConfigurationOptions, RedisConnectionPoolSettings connectionPoolSettings)
        {
            _pool = new IConnectionMultiplexer[connectionPoolSettings.MaxPoolSize];
            _redisConfigurationOptions = redisConfigurationOptions;
            _lock = new SemaphoreSlim(1, 1);
        }

        public async Task<IDatabase> GetDatabaseAsync()
        {                
            var connection = await GetConnectionMultiplexerAsync().ConfigureAwait(false);
            return connection.GetDatabase();              
        }

        private async Task<IConnectionMultiplexer> GetConnectionMultiplexerAsync()
        {
            var leastPendingTasks = long.MaxValue;
            var leastPendingIndex = -1;
            int firstUnusedIndex = -1;

            for (int i = 0; i < _pool.Length; i++)
            {
                var connection = _pool[i];

                if (firstUnusedIndex == -1 && connection == null)
                {
                    firstUnusedIndex = i;
                }

                if (connection != null)
                {
                    var pending = connection.GetCounters().TotalOutstanding;

                    if (pending == 0)
                    {
                        return connection;
                    }

                    if (pending < leastPendingTasks)
                    {
                        leastPendingTasks = pending;
                        leastPendingIndex = i;
                    }
                }
            }

            if (firstUnusedIndex > -1)
            {
                try
                {
                    await _lock.WaitAsync().ConfigureAwait(false);
                    if (_pool[firstUnusedIndex] == null)
                    {
                        _pool[firstUnusedIndex] = await ConnectionMultiplexer.ConnectAsync(_redisConfigurationOptions).ConfigureAwait(false);
                    }

                    return _pool[firstUnusedIndex];
                }
                finally
                {
                    _lock.Release();
                }                
            }
            if (leastPendingIndex > -1)
            {
                return _pool[leastPendingIndex];
            }
            
            throw new InvalidOperationException("Could not get valid index from redis connection pool");            
        }
    }
}
