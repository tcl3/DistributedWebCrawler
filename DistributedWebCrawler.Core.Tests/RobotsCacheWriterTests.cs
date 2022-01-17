using AutoFixture.Xunit2;
using DistributedWebCrawler.Core.Interfaces;
using DistributedWebCrawler.Core.Robots;
using DistributedWebCrawler.Core.Tests.Attributes;
using Moq;
using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace DistributedWebCrawler.Core.Tests
{
    public class RobotsCacheWriterTests
    {
        private const string MockRobotsContent = "MockRobotsContent";
        private static readonly Uri MockUri = new Uri("http://mock.test/");

        private readonly CancellationTokenSource _cts = new(TimeSpan.FromSeconds(1));

        [HttpClientAutoData(content: MockRobotsContent)]
        [Theory]
        public async Task AddRobotsShouldReturnContentWhenRequestIsSuccessful(
            [Frozen] Mock<IKeyValueStore> keyValueStoreMock,
            RobotsCacheWriter sut)
        {
            var expireAfter = TimeSpan.FromTicks(1);

            var content = await sut.AddOrUpdateRobotsForHostAsync(MockUri, expireAfter, _cts.Token);
            
            keyValueStoreMock.Verify(x => x.PutAsync(It.IsAny<string>(), MockRobotsContent, expireAfter), Times.Once());
            Assert.Equal(MockRobotsContent, content);
        }

        [HttpClientAutoData(statusCode: HttpStatusCode.NotFound)]
        [Theory]
        public async Task AddRobotsShouldReturnEmptyStringWhenRequestFails(
            [Frozen] Mock<IKeyValueStore> keyValueStoreMock,
            RobotsCacheWriter sut)
        {
            var expireAfter = TimeSpan.FromTicks(1);

            var content = await sut.AddOrUpdateRobotsForHostAsync(MockUri, expireAfter, _cts.Token);

            keyValueStoreMock.Verify(x => x.PutAsync(It.IsAny<string>(), string.Empty, expireAfter), Times.Once());
            Assert.Equal(string.Empty, content);
        }
    }
}
