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
            [Frozen] Mock<IKeyValueStore> keyValueStore,
            [Frozen] SchedulerRequest schedulerRequest,
            [Frozen] RobotsRequest request, 
            RobotsDownloaderRequestProcessor sut)
        {
            // FIXME: This was added because Moq cannot construct SchedulerRequest by itself
            // Surely setup like this can be automated though?
            keyValueStore.Setup(x => x.GetAsync<SchedulerRequest>(It.IsAny<string>()))
                .ReturnsAsync(schedulerRequest);

            var result = await sut.ProcessItemAsync(request);
            Assert.IsAssignableFrom<QueuedItemResult<RobotsDownloaderSuccess>>(result);
        }
    }
}
