using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DistributedWebCrawler.Core.Configuration
{
    public class SchedulerSettings : TaskQueueSettings
    {

        [Required]
        public virtual bool? RespectsRobotsTxt { get; init; }

        [Range(0, int.MaxValue, ErrorMessage = nameof(MaxCrawlDepth) + " must be a non-negative integer")]
        public virtual int? MaxCrawlDepth { get; init; }
        
        [Range(0, int.MaxValue, ErrorMessage = nameof(MaxCrawlDepth) + " must be a positive integer")]
        public virtual int SameDomainCrawlDelayMillis { get; init; }
        
        public virtual IEnumerable<string>? ExcludeDomains { get; init; } 
        public virtual IEnumerable<string>? IncludeDomains { get; init; }
    }
}
