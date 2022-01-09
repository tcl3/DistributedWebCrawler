using StackExchange.Redis;

namespace DistributedWebCrawler.Extensions.Redis
{
    public interface IConnectionMultiplexerPool
    {
        Task<IDatabase> GetDatabaseAsync();
    }
}
