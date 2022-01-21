using AutoFixture.Xunit2;
using DistributedWebCrawler.Core.Interfaces;
using DistributedWebCrawler.Core.Robots;
using DistributedWebCrawler.Core.Tests.Attributes;
using Moq;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace DistributedWebCrawler.Core.Tests
{
    public class RobotsCacheReaderTests
    {
        private const string MockRobotsTxtContent = "MockRobotsTxtContent";

        [Theory]
        [RobotsAutodata(robotsContent: null, expectedResult: false)]
        public async Task GetRobotsTextShouldReturnFalseWhenUriNotCached(
            string robotsContent,
            bool expectedResult,
            Uri uri,
            [Frozen] Mock<IKeyValueStore> keyValueStoreMock, 
            RobotsCacheReader sut)
        {
            await RobotsTxtContentTest(robotsContent, expectedResult, uri, keyValueStoreMock, sut);
        }

        [Theory]
        [RobotsAutodata(robotsContent: MockRobotsTxtContent, expectedResult: true)]
        public async Task GetRobotsTextShouldReturnTrueWhenUriIsCached(
            string robotsContent,
            bool expectedResult,
            Uri uri,
            [Frozen] Mock<IKeyValueStore> keyValueStoreMock, 
            RobotsCacheReader sut)
        {
            await RobotsTxtContentTest(robotsContent, expectedResult, uri, keyValueStoreMock, sut);
        }

        [Theory]
        [RobotsAutodata(robotsContent: "Allow: /", expectedResult: true)]
        public async Task AllowedShouldReturnTrueWhenUriIsAllowedInRobotsTxt(
            string robotsContent,
            bool expectedResult,
            Uri uri,
            [Frozen] Mock<IKeyValueStore> keyValueStoreMock, 
            RobotsCacheReader sut)
        {
            await RobotsTxtContentTest(robotsContent, expectedResult, uri, keyValueStoreMock, sut, robots =>
            {
                Assert.True(robots.Allowed(uri));
            });
        }

        [Theory]
        [RobotsAutodata(robotsContent: "User-agent: *\nDisallow: /", expectedResult: true)]
        public async Task AllowedShouldReturnFalseWhenUriIsDisallowedInRobotsTxt(
            string robotsContent,
            bool expectedResult,
            Uri uri,
            [Frozen] Mock<IKeyValueStore> keyValueStoreMock,
            RobotsCacheReader sut)
        {
            await RobotsTxtContentTest(robotsContent, expectedResult, uri, keyValueStoreMock, sut, robots =>
            {
                Assert.False(robots.Allowed(new Uri("/", UriKind.Relative)));
            });
        }

        [Theory]
        [RobotsAutodata(robotsContent: "User-agent: *\nCrawl-Delay: 10", expectedResult: true)]
        public async Task EnsureCrawlDelayPropertyIsPopulatedWhenRobotsTxtContainsCrawlDelay(
            string robotsContent,
            bool expectedResult,
            Uri uri,
            [Frozen] Mock<IKeyValueStore> keyValueStoreMock,
            RobotsCacheReader sut)
        {
            await RobotsTxtContentTest(robotsContent, expectedResult, uri, keyValueStoreMock, sut, robots =>
            {
                Assert.Equal(10, robots.CrawlDelay);
            });
        }

        [Theory]
        [RobotsAutodata(robotsContent: "User-agent: *\nSitemap: http://test.com/sitemap.xml", expectedResult: true)]
        public async Task EnsureSitemapUrlsPropertyIsPopulatedWhenRobotsTxtContainsSitemaps(
            string robotsContent,
            bool expectedResult,
            Uri uri,
            [Frozen] Mock<IKeyValueStore> keyValueStoreMock,
            RobotsCacheReader sut)
        {
            await RobotsTxtContentTest(robotsContent, expectedResult, uri, keyValueStoreMock, sut, robots =>
            {
                Assert.Single(robots.SitemapUrls);
                Assert.Equal("http://test.com/sitemap.xml", robots.SitemapUrls.First());
            });            
        }

        private async Task RobotsTxtContentTest(
            string robotsContent,
            bool expectedResult,
            Uri uri,
            Mock<IKeyValueStore> keyValueStoreMock,
            RobotsCacheReader sut,
            Action<IRobots>? additionalRobotsAssertions = null)
        {
            keyValueStoreMock.Setup(x => x.GetAsync(It.IsAny<string>()))
               .ReturnsAsync(() => robotsContent);

            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(1));

            var ifExistsActionCalled = false;
            var result = await sut.GetRobotsTxtAsync(uri, robots =>
            {
                ifExistsActionCalled = true;
                additionalRobotsAssertions?.Invoke(robots);
            }, cts.Token);

            Assert.Equal(expectedResult, result); ;
            Assert.Equal(expectedResult, ifExistsActionCalled);

            keyValueStoreMock.Verify(x => x.GetAsync(It.IsAny<string>()), Times.Once());
        }
    }
}
