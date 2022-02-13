using AutoFixture;
using DistributedWebCrawler.Core.Configuration;
using DistributedWebCrawler.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DistributedWebCrawler.Core.Tests.Customizations
{
    internal class SchedulerSettingsCustomization : ICustomization
    {
        private readonly Uri? _uri;
        private readonly string[]? _paths;
        private readonly int? _currentCrawlDepth;
        private readonly bool _respectsRobotsTxt;
        private readonly string[]? _includeDomains;
        private readonly string[]? _excludeDomains;
        private readonly int _sameDomainCrawlDelayMillis;
        private readonly int _maxCrawlDepth;
        private readonly int _maxConcurrentItems;

        public SchedulerSettingsCustomization(
            Uri? uri = null,
            string[]? paths = null,
            int? currentCrawlDepth = null,
            bool respectsRobotsTxt = false,
            string[]? includeDomains = null,
            string[]? excludeDomains = null,
            int sameDomainCrawlDelayMillis = 1,
            int maxCrawlDepth = 1,
            int maxConcurrentItems = 1)
        {
            _uri = uri;
            _paths = paths;
            _currentCrawlDepth = currentCrawlDepth;
            _respectsRobotsTxt = respectsRobotsTxt;
            _includeDomains = includeDomains;
            _excludeDomains = excludeDomains;
            _sameDomainCrawlDelayMillis = sameDomainCrawlDelayMillis;
            _maxCrawlDepth = maxCrawlDepth;
            _maxConcurrentItems = maxConcurrentItems;
        }

        public void Customize(IFixture fixture)
        {
            fixture.Customize<SchedulerRequest>(composer =>
            {
                var result = composer
                    .With(x => x.Paths, _paths)
                    .With(x => x.CurrentCrawlDepth, _currentCrawlDepth);

                result = _uri != null
                    ? result.With(x => x.Uri, _uri)
                    : result.With(x => x.Uri);

                return result;
            });

            fixture.Customize<SchedulerSettings>(composer => composer
                .With(x => x.MaxConcurrentItems, _maxConcurrentItems)
                .With(x => x.MaxCrawlDepth, _maxCrawlDepth)
                .With(x => x.RespectsRobotsTxt, _respectsRobotsTxt)
                .With(x => x.SameDomainCrawlDelayMillis, _sameDomainCrawlDelayMillis)
                .With(x => x.IncludeDomains, _includeDomains)
                .With(x => x.ExcludeDomains, _excludeDomains)
            );
        }
    }
}
