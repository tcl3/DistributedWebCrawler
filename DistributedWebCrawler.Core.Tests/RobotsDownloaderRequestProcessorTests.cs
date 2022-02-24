using AutoFixture;
using AutoFixture.Xunit2;
using DistributedWebCrawler.Core.Interfaces;
using DistributedWebCrawler.Core.Models;
using DistributedWebCrawler.Core.RequestProcessors;
using DistributedWebCrawler.Core.Tests.Attributes;
using DistributedWebCrawler.Core.Tests.Collections;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace DistributedWebCrawler.Core.Tests
{
    [Collection(nameof(RequestProcessorCollection))]
    public class RobotsDownloaderRequestProcessorTests
    {
        [Theory]
        [MoqAutoData(configureMembers: true)]
        public async Task ProcessItemShouldReturnSuccess(
            [Frozen] Mock<IKeyValueStore> keyValueStoreMock,
            [Frozen] Mock<IProducer<SchedulerRequest>> schedulerRequestProducerMock,
            [Frozen] SchedulerRequest schedulerRequest,
            [Frozen] RobotsRequest request, 
            RobotsDownloaderRequestProcessor sut)
        {
            // FIXME: This was added because Moq cannot construct SchedulerRequest by itself
            // Surely setup like this can be automated though?
            keyValueStoreMock.Setup(x => x.GetAsync<SchedulerRequest>(It.IsAny<string>()))
                .ReturnsAsync(schedulerRequest);

            var result = await sut.ProcessItemAsync(request);

            Assert.IsAssignableFrom<QueuedItemResult<RobotsDownloaderSuccess>>(result);
            keyValueStoreMock.Verify(x => x.GetAsync<SchedulerRequest>(It.IsAny<string>()), Times.Once());
            keyValueStoreMock.Verify(x => x.RemoveAsync(It.IsAny<string>()), Times.Once());
            schedulerRequestProducerMock.Verify(x => x.Enqueue(schedulerRequest), Times.Once());
        }
    }
}
