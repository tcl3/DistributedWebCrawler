using AutoFixture.Xunit2;
using DistributedWebCrawler.Core.Interfaces;
using DistributedWebCrawler.Core.Tests.Attributes;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace DistributedWebCrawler.Core.Tests
{
    public class ContentStoreTests
    {
        private const string TestContent = "TestContent";
        private readonly CancellationTokenSource _cts = new(TimeSpan.FromSeconds(1));

        [Theory]
        [MoqAutoData]
        public async Task GetTest([Frozen] Mock<IKeyValueStore> keyValueStoreMock, ContentStore sut, Guid id)
        {
            var resultContent = await sut.GetContentAsync(id, _cts.Token);
            keyValueStoreMock.Verify(x => x.GetAsync(It.IsAny<string>()), Times.Once());
        }

        [Theory]
        [MoqAutoData]
        public async Task PutTest([Frozen] Mock<IKeyValueStore> keyValueStoreMock, ContentStore sut)
        {
            var id = await sut.SaveContentAsync(TestContent, _cts.Token);
            keyValueStoreMock.Verify(x => x.PutAsync(It.IsAny<string>(), TestContent, null), Times.Once());
        }

        [Theory]
        [MoqAutoData]
        public async Task RemoveTest([Frozen] Mock<IKeyValueStore> keyValueStoreMock, ContentStore sut, Guid id)
        {
            await sut.RemoveAsync(id, _cts.Token);
            keyValueStoreMock.Verify(x => x.RemoveAsync(It.IsAny<string>()), Times.Once());
        }
    }
}
