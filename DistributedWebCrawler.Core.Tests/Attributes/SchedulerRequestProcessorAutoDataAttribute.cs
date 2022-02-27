using AutoFixture;
using DistributedWebCrawler.Core.Tests.Customizations;
using System;

namespace DistributedWebCrawler.Core.Tests.Attributes
{
    internal class SchedulerRequestProcessorAutoDataAttribute : MoqAutoDataAttribute
    {
        public SchedulerRequestProcessorAutoDataAttribute(
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
            : base(new ICustomization[]
            {
                new SchedulerRequestProcessorCustomization(
                    allowedByRobots,
                    robotsContentExists),
                new SchedulerSettingsCustomization(
                    uri: uri == null ? null : new Uri(uri, UriKind.Absolute),
                    paths: paths ?? new[] {"/"},
                    currentCrawlDepth: currentCrawlDepth,
                    respectsRobotsTxt: respectsRobotsTxt ? true : null,
                    includeDomains: includeDomains,
                    excludeDomains: excludeDomains,
                    maxCrawlDepth: maxCrawlDepth,
                    maxConcurrentItems: maxConcurrentItems),
            },
            configureMembers: true)
        {
        }
    }
}
