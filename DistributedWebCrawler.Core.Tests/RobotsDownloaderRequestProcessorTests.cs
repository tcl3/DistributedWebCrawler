using AutoFixture.Xunit2;
using DistributedWebCrawler.Core.Model;
using DistributedWebCrawler.Core.Models;
using DistributedWebCrawler.Core.RequestProcessors;
using DistributedWebCrawler.Core.Tests.Attributes;
using DistributedWebCrawler.Core.Tests.Collections;
using System.Threading.Tasks;
using Xunit;

namespace DistributedWebCrawler.Core.Tests
{
    [Collection(nameof(RequestProcessorCollection))]
    public class RobotsDownloaderRequestProcessorTests
    {
        [Theory]
        [MoqAutoData(configureMembers: true)]
        public async Task ProcessItemShouldReturnSuccess([Frozen] RobotsRequest request, RobotsDownloaderRequestProcessor sut)
        {
            var result = await sut.ProcessItemAsync(request);
            Assert.IsAssignableFrom<QueuedItemResult<RobotsDownloaderSuccess>>(result);
        }
    }
}
