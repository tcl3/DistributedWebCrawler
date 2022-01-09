using System.ComponentModel.DataAnnotations;

namespace DistributedWebCrawler.Extensions.Redis
{
    public class RedisConnectionPoolSettings
    {
        [Range(1, int.MaxValue, ErrorMessage = nameof(MaxPoolSize) + " must be a positive integer")]
        public int MaxPoolSize { get; init; }
    }
}
