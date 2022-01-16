using System.ComponentModel.DataAnnotations;

namespace DistributedWebCrawler.Core.Configuration
{
    public class RobotsTxtSettings : TaskQueueSettings
    {
        [Range(1, int.MaxValue, ErrorMessage = nameof(CacheIntervalSeconds) + " must be a positive integer")]
        public virtual int CacheIntervalSeconds { get; init; }
    }
}
