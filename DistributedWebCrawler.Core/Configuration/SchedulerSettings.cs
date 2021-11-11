using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace DistributedWebCrawler.Core.Configuration
{
    public class SchedulerSettings
    {
        [Range(1, int.MaxValue)]
        public int MaxConcurrentRobotsRequests { get; set; }

        [Required]
        public bool? RespectsRobotsTxt { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = nameof(MaxCrawlDepth) + " must be a non-negative integer")]
        public int? MaxCrawlDepth { get; set; }
        
        [Range(0, int.MaxValue, ErrorMessage = nameof(MaxCrawlDepth) + " must be a positive integer")]
        public int SameDomainCrawlDelayMillis { get; set; }
        
        public IEnumerable<string> ExcludeUris { get; set; } = Enumerable.Empty<string>();
        public IEnumerable<string> IncludeUris { get; set; } = Enumerable.Empty<string>();
    }
}
