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

        [Theory]
        [MoqAutoData]
        public async Task GetContentShouldCallKeyValueStore([Frozen] Mock<IKeyValueStore> keyValueStoreMock, ContentStore sut, Guid id)
        {            
            var resultContent = await sut.GetContentAsync(id);
            keyValueStoreMock.Verify(x => x.GetAsync(It.IsAny<string>()), Times.Once());
        }

        [Theory]
        [MoqAutoData]
        public async Task SaveContentShouldCallKeyValueStore([Frozen] Mock<IKeyValueStore> keyValueStoreMock, ContentStore sut)
        {
            var id = await sut.SaveContentAsync(TestContent);
            keyValueStoreMock.Verify(x => x.PutAsync(It.IsAny<string>(), TestContent, null), Times.Once());
        }

        [Theory]
        [MoqAutoData]
        public async Task RemoveShouldCallKeyValueStore([Frozen] Mock<IKeyValueStore> keyValueStoreMock, ContentStore sut, Guid id)
        {
            await sut.RemoveAsync(id);
            keyValueStoreMock.Verify(x => x.RemoveAsync(It.IsAny<string>()), Times.Once());
        }
    }
}
