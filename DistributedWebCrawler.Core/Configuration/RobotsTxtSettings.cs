using System.ComponentModel.DataAnnotations;

namespace DistributedWebCrawler.Core.Configuration
{
    public class RobotsTxtSettings : TaskQueueSettings
    {
        [Range(1, int.MaxValue, ErrorMessage = nameof(CacheIntervalSeconds) + " must be a positive integer")]
        public int CacheIntervalSeconds { get; init; }
        
        [Range(1, int.MaxValue, ErrorMessage = nameof(MaxConcurrentRobotsRequests) + " must be a positive integer")]
        public int MaxConcurrentRobotsRequests { get; init; }

        internal override int MaxConcurrentItems => MaxConcurrentRobotsRequests;
    }
}
