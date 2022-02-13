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
        private readonly bool _allowedByRobots;
        private readonly bool _robotsContentExists;

        public SchedulerRequestProcessorCustomization(
            bool allowedByRobots = true,
            bool robotsContentExists = true)
        {
            _allowedByRobots = allowedByRobots;
            _robotsContentExists = robotsContentExists;
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
        }
    }
}
