using System.ComponentModel.DataAnnotations;

namespace DistributedWebCrawler.Core.Configuration
{
    public class RobotsTxtSettings
    {
        [Range(1, int.MaxValue, ErrorMessage = nameof(CacheIntervalSeconds) + " must be a positive integer")]
        public int CacheIntervalSeconds { get; set; }
    }
}
