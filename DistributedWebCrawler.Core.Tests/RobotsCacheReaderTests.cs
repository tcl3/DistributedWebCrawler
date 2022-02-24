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
        [RobotsAutodata(robotsContent: null, expectedResult: false, shouldInvokeCallback: false)]
        public async Task GetRobotsTextShouldReturnFalseWhenUriNotCached(
            string robotsContent,
            bool expectedResult,
            bool shouldInvokeCallback,
            Uri uri,
            [Frozen] Mock<IKeyValueStore> keyValueStoreMock, 
            RobotsCacheReader sut)
        {
            await RobotsTxtContentTest(robotsContent, expectedResult, shouldInvokeCallback, uri, keyValueStoreMock, sut);
        }

        [Theory]
        [RobotsAutodata(robotsContent: "", expectedResult: true, shouldInvokeCallback: false)]
        public async Task GetRobotsTextShouldReturnFalseWhenUriNotCached2(
            string robotsContent,
            bool expectedResult,
            bool shouldInvokeCallback,
            Uri uri,
            [Frozen] Mock<IKeyValueStore> keyValueStoreMock,
            RobotsCacheReader sut)
        {
            await RobotsTxtContentTest(robotsContent, expectedResult, shouldInvokeCallback, uri, keyValueStoreMock, sut);
        }

        [Theory]
        [RobotsAutodata(robotsContent: MockRobotsTxtContent, expectedResult: true, shouldInvokeCallback: true)]
        public async Task GetRobotsTextShouldReturnTrueWhenUriIsCached(
            string robotsContent,
            bool expectedResult,
            bool shouldInvokeCallback,
            Uri uri,
            [Frozen] Mock<IKeyValueStore> keyValueStoreMock, 
            RobotsCacheReader sut)
        {
            await RobotsTxtContentTest(robotsContent, expectedResult, shouldInvokeCallback, uri, keyValueStoreMock, sut);
        }

        [Theory]
        [RobotsAutodata(robotsContent: "Allow: /", expectedResult: true, shouldInvokeCallback: true)]
        public async Task AllowedShouldReturnTrueWhenUriIsAllowedInRobotsTxt(
            string robotsContent,
            bool expectedResult,
            bool shouldInvokeCallback,
            Uri uri,
            [Frozen] Mock<IKeyValueStore> keyValueStoreMock, 
            RobotsCacheReader sut)
        {
            await RobotsTxtContentTest(robotsContent, expectedResult, shouldInvokeCallback, uri, keyValueStoreMock, sut, robots =>
            {
                Assert.True(robots.Allowed(uri));
            });
        }

        [Theory]
        [RobotsAutodata(robotsContent: "User-agent: *\nDisallow: /", expectedResult: true, shouldInvokeCallback: true)]
        public async Task AllowedShouldReturnFalseWhenUriIsDisallowedInRobotsTxt(
            string robotsContent,
            bool expectedResult,
            bool shouldInvokeCallback,
            Uri uri,
            [Frozen] Mock<IKeyValueStore> keyValueStoreMock,
            RobotsCacheReader sut)
        {
            await RobotsTxtContentTest(robotsContent, expectedResult, shouldInvokeCallback, uri, keyValueStoreMock, sut, robots =>
            {
                Assert.False(robots.Allowed(new Uri("/", UriKind.Relative)));
            });
        }

        [Theory]
        [RobotsAutodata(robotsContent: "User-agent: *\nCrawl-Delay: 10", expectedResult: true, shouldInvokeCallback: true)]
        public async Task EnsureCrawlDelayPropertyIsPopulatedWhenRobotsTxtContainsCrawlDelay(
            string robotsContent,
            bool expectedResult,
            bool shouldInvokeCallback,
            Uri uri,
            [Frozen] Mock<IKeyValueStore> keyValueStoreMock,
            RobotsCacheReader sut)
        {
            await RobotsTxtContentTest(robotsContent, expectedResult, shouldInvokeCallback, uri, keyValueStoreMock, sut, robots =>
            {
                Assert.Equal(10, robots.CrawlDelay);
            });
        }

        [Theory]
        [RobotsAutodata(robotsContent: "User-agent: *\nSitemap: http://test.com/sitemap.xml", expectedResult: true, shouldInvokeCallback: true)]
        public async Task EnsureSitemapUrlsPropertyIsPopulatedWhenRobotsTxtContainsSitemaps(
            string robotsContent,
            bool expectedResult,
            bool shouldInvokeCallback,
            Uri uri,
            [Frozen] Mock<IKeyValueStore> keyValueStoreMock,
            RobotsCacheReader sut)
        {
            await RobotsTxtContentTest(robotsContent, expectedResult, shouldInvokeCallback, uri, keyValueStoreMock, sut, robots =>
            {
                Assert.Single(robots.SitemapUrls);
                Assert.Equal("http://test.com/sitemap.xml", robots.SitemapUrls.First());
            });            
        }

        private static async Task RobotsTxtContentTest(
            string robotsContent,
            bool expectedResult,
            bool shouldInvokeCallback,
            Uri uri,
            Mock<IKeyValueStore> keyValueStoreMock,
            RobotsCacheReader sut,
            Action<IRobots>? additionalRobotsAssertions = null)
        {
            keyValueStoreMock.Setup(x => x.GetAsync(It.IsAny<string>()))
               .ReturnsAsync(() => robotsContent);

            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(1));

            var ifExistsActionCalled = false;

            var resultWithNullAction = await sut.GetRobotsTxtAsync(uri, null, cts.Token);

            var result = await sut.GetRobotsTxtAsync(uri, robots =>
            {
                ifExistsActionCalled = true;
                additionalRobotsAssertions?.Invoke(robots);
            }, cts.Token);

            Assert.Equal(expectedResult, resultWithNullAction);
            Assert.Equal(expectedResult, result);
            Assert.Equal(shouldInvokeCallback, ifExistsActionCalled);

            keyValueStoreMock.Verify(x => x.GetAsync(It.IsAny<string>()), Times.Exactly(2));
        }
    }
}
