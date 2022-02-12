using DistributedWebCrawler.Core.Tests.Customizations;
using System;

namespace DistributedWebCrawler.Core.Tests.Attributes
{
    internal class SchedulerAutoDataAttribute : MoqAutoDataAttribute
    {
        public SchedulerAutoDataAttribute(
            string? uri = null,
            string[]? paths = null,
            int currentCrawlDepth = 0,
            bool respectsRobotsTxt = false,
            bool allowedByRobots = true,
            bool robotsContentExists = true,
            string[]? includeDomains = null,
            string[]? excludeDomains = null,
            int maxCrawlDepth = 1,
            int maxConcurrentItems = 1
            )
            : base(new SchedulerRequestProcessorCustomization(
                uri == null ? null : new Uri(uri, UriKind.Absolute), 
                paths ?? new[] {"/"}, 
                currentCrawlDepth, 
                respectsRobotsTxt, 
                allowedByRobots, 
                robotsContentExists, 
                includeDomains, 
                excludeDomains, 
                maxCrawlDepth, 
                maxConcurrentItems), 
            configureMembers: true)
        {
        }
    }
}
