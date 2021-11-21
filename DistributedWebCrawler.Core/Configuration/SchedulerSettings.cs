using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace DistributedWebCrawler.Core.Configuration
{
    public class SchedulerSettings
    {
        [Range(1, int.MaxValue)]
        public int MaxConcurrentRobotsRequests { get; init; }

        [Required]
        public bool? RespectsRobotsTxt { get; init; }

        [Range(0, int.MaxValue, ErrorMessage = nameof(MaxCrawlDepth) + " must be a non-negative integer")]
        public int? MaxCrawlDepth { get; init; }
        
        [Range(0, int.MaxValue, ErrorMessage = nameof(MaxCrawlDepth) + " must be a positive integer")]
        public int SameDomainCrawlDelayMillis { get; init; }
        
        public IEnumerable<string> ExcludeUris { get; init; } = Enumerable.Empty<string>();
        public IEnumerable<string> IncludeUris { get; init; } = Enumerable.Empty<string>();
    }
}
