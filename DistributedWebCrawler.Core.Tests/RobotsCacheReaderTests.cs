using AutoFixture.Xunit2;
using DistributedWebCrawler.Core.Interfaces;
using DistributedWebCrawler.Core.Robots;
using DistributedWebCrawler.Core.Tests.Attributes;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace DistributedWebCrawler.Core.Tests
{
    public class RobotsCacheReaderTests
    {
        private static readonly Uri MockUri = new Uri("http://mock.test/");
        private const string MockRobotsTxtContent = "MockRobotsTxtContent";

        private readonly CancellationTokenSource _cts = new(TimeSpan.FromSeconds(1));

        [Theory]
        [MoqAutoData]
        public async Task GetRobotsTextShouldReturnFalseWhenUriNotCached([Frozen] Mock<IKeyValueStore> keyValueStoreMock, RobotsCacheReader sut)
        {
            keyValueStoreMock.Setup(x => x.GetAsync(It.IsAny<string>()))
                .ReturnsAsync(() => null);

            var ifExistsActionCalled = false;
            var result = await sut.GetRobotsTxtAsync(MockUri, robots => ifExistsActionCalled = true, _cts.Token);
            Assert.False(result);
            Assert.False(ifExistsActionCalled);

            keyValueStoreMock.Verify(x => x.GetAsync(It.IsAny<string>()), Times.Once());
        }

        [Theory]
        [MoqAutoData]
        public async Task GetRobotsTextShouldReturnTrueWhenUriIsCached([Frozen] Mock<IKeyValueStore> keyValueStoreMock, RobotsCacheReader sut)
        {
            keyValueStoreMock.Setup(x => x.GetAsync(It.IsAny<string>()))
                .ReturnsAsync(() => MockRobotsTxtContent);

            var ifExistsActionCalled = false;
            var result = await sut.GetRobotsTxtAsync(MockUri, robots => ifExistsActionCalled = true, _cts.Token);
            Assert.True(result);
            Assert.True(ifExistsActionCalled);


            keyValueStoreMock.Verify(x => x.GetAsync(It.IsAny<string>()), Times.Once());
        }
    }
}
