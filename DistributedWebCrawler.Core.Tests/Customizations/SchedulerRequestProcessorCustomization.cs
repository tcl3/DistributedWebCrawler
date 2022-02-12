using AutoFixture;
using DistributedWebCrawler.Core.Configuration;
using DistributedWebCrawler.Core.Interfaces;
using DistributedWebCrawler.Core.Models;
using Moq;
using Nager.PublicSuffix;
using System;
using System.Threading;

namespace DistributedWebCrawler.Core.Tests.Customizations
{
    internal class SchedulerRequestProcessorCustomization : ICustomization
    {
        private readonly Uri? _uri;
        private readonly string[]? _paths;
        private readonly int? _currentCrawlDepth;
        private readonly bool _respectsRobotsTxt;
        private readonly bool _allowedByRobots;
        private readonly bool _robotsContentExists;
        private readonly string[]? _includeDomains;
        private readonly string[]? _excludeDomains;
        private readonly int _maxCrawlDepth;
        private readonly int _maxConcurrentItems;

        public SchedulerRequestProcessorCustomization(
            Uri? uri = null,
            string[]? paths = null,
            int? currentCrawlDepth = null,
            bool respectsRobotsTxt = false,
            bool allowedByRobots = true,
            bool robotsContentExists = true,
            string[]? includeDomains = null,
            string[]? excludeDomains = null,
            int maxCrawlDepth = 1,
            int maxConcurrentItems = 1)

        {
            _uri = uri;
            _paths = paths;
            _currentCrawlDepth = currentCrawlDepth;
            _respectsRobotsTxt = respectsRobotsTxt;
            _allowedByRobots = allowedByRobots;
            _robotsContentExists = robotsContentExists;
            _includeDomains = includeDomains;
            _excludeDomains = excludeDomains;
            _maxCrawlDepth = maxCrawlDepth;
            _maxConcurrentItems = maxConcurrentItems;
        }

        public void Customize(IFixture fixture)
        {
            var domainParserMock = new Mock<IDomainParser>();
            domainParserMock.Setup(x => x.IsValidDomain(It.IsAny<string>())).Returns(() => false);

            fixture.Inject(domainParserMock.Object);

            var robotsMock = new Mock<IRobots>();

            robotsMock.Setup(x => x.Allowed(It.IsAny<Uri>()))
                .Returns(() => _allowedByRobots);

            fixture.Inject(robotsMock);

            var robotsCacheWriterMock = new Mock<IRobotsCacheReader>();

            robotsCacheWriterMock.Setup(x => 
                x.GetRobotsTxtAsync(
                    It.IsAny<Uri>(), 
                    It.IsAny<Action<IRobots>?>(),
                    It.IsAny<CancellationToken>()))
                .Callback<Uri, Action<IRobots>, CancellationToken>((_, action, _) =>
                {
                    if (action != null)
                    {
                        var robots = fixture.Create<IRobots>();
                        action(robots);
                    }                    
                })
                .ReturnsAsync(() => _robotsContentExists);

            fixture.Inject(robotsCacheWriterMock);

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
                .With(x => x.IncludeDomains, _includeDomains)
                .With(x => x.ExcludeDomains, _excludeDomains)
            );
        }
    }
}
