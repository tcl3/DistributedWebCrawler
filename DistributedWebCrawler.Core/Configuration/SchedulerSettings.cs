using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DistributedWebCrawler.Core.Configuration
{
    public class SchedulerSettings : TaskQueueSettings
    {
        [Range(1, int.MaxValue)]
        public int MaxConcurrentRobotsRequests { get; init; }

        [Required]
        public bool? RespectsRobotsTxt { get; init; }

        [Range(0, int.MaxValue, ErrorMessage = nameof(MaxCrawlDepth) + " must be a non-negative integer")]
        public int? MaxCrawlDepth { get; init; }
        
        [Range(0, int.MaxValue, ErrorMessage = nameof(MaxCrawlDepth) + " must be a positive integer")]
        public int SameDomainCrawlDelayMillis { get; init; }
        
        public IEnumerable<string>? ExcludeDomains { get; init; } 
        public IEnumerable<string>? IncludeDomains { get; init; }

        internal override int MaxConcurrentItems => MaxConcurrentRobotsRequests;
    }
}
