using AutoFixture.Xunit2;
using DistributedWebCrawler.Core.Models;
using DistributedWebCrawler.Core.RequestProcessors;
using DistributedWebCrawler.Core.Tests.Attributes;
using DistributedWebCrawler.Core.Tests.Collections;
using FluentAssertions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace DistributedWebCrawler.Core.Tests
{
    [Collection(nameof(RequestProcessorCollection))]
    public class SchedulerRequestProcessorTests
    {
        [Theory]
        [SchedulerRequestProcessorAutoData]
        public async Task ProcessItemShouldQueueAllPathsInRequest(
            [Frozen] SchedulerRequest request, 
            SchedulerRequestProcessor sut)
        {
            var result = await sut.ProcessItemAsync(request);
            var successResult = Assert.IsAssignableFrom<QueuedItemResult<SchedulerSuccess>>(result);
            successResult.Result.AddedPaths.Should().BeEquivalentTo(request.Paths);
        }

        [Theory]
        [SchedulerRequestProcessorAutoData(paths: new[] { "/", "/test", "/test2", "/test3", "/" })]
        public async Task ProcessItemShouldQueueAllPathsInRequestForRequestWithMultiplePaths(
            [Frozen] SchedulerRequest request, 
            SchedulerRequestProcessor sut)
        {
            var result = await sut.ProcessItemAsync(request);

            var success = Assert.IsAssignableFrom<QueuedItemResult<SchedulerSuccess>>(result);
            success.Result.AddedPaths.Should().BeEquivalentTo(request.Paths);
        }

        [Theory]
        [SchedulerRequestProcessorAutoData]
        public async Task ProcessItemShouldNotQueuePreviouslyQueuedPaths(
            [Frozen] SchedulerRequest request, 
            SchedulerRequestProcessor sut)
        {
            var result1 = await sut.ProcessItemAsync(request);
            var successResult1 = Assert.IsAssignableFrom<QueuedItemResult<SchedulerSuccess>>(result1);
            successResult1.Result.AddedPaths.Should().BeEquivalentTo(request.Paths);

            var result2 = await sut.ProcessItemAsync(request);
            var successResult2 = Assert.IsAssignableFrom<QueuedItemResult<SchedulerSuccess>>(result2);
            Assert.Empty(successResult2.Result.AddedPaths);
        }

        [Theory]
        [SchedulerRequestProcessorAutoData]
        public async Task ProcessItemShouldNotQueuePreviouslyQueuedPathsWhenInvokedInParallel(
            [Frozen] SchedulerRequest request,
            SchedulerRequestProcessor sut)
        {
            var resultTask1 = sut.ProcessItemAsync(request);
            var resultTask2 = sut.ProcessItemAsync(request);

            var results = await Task.WhenAll(new[] { resultTask1, resultTask2 });

            var allAddedPaths = new List<string>();
            foreach (var result in results)
            {
                var successResult = Assert.IsAssignableFrom<QueuedItemResult<SchedulerSuccess>>(result);
                allAddedPaths.AddRange(successResult.Result.AddedPaths);
            }

            allAddedPaths.Should().BeEquivalentTo(request.Paths);
        }

        [Theory]
        [SchedulerRequestProcessorAutoData(currentCrawlDepth: 2, maxCrawlDepth: 1)]
        public async Task ProcessItemShouldReturnFailureWhenCrawlDepthExceeded(
            [Frozen] SchedulerRequest request,
            SchedulerRequestProcessor sut)
        {
            var result = await sut.ProcessItemAsync(request);

            var failure = Assert.IsAssignableFrom<QueuedItemResult<ErrorCode<SchedulerFailure>>>(result);
            Assert.Equal(SchedulerFailure.MaximumCrawlDepthReached, failure.Result.Error);
        }

        [Theory]
        [SchedulerRequestProcessorAutoData(respectsRobotsTxt: true, allowedByRobots: true)]
        public async Task ProcessItemShouldQueueAllPathsInRequestWhenPathAllowedByRobotsTxt(
            [Frozen] SchedulerRequest request,
            SchedulerRequestProcessor sut)
        {
            var result = await sut.ProcessItemAsync(request);
            var successResult = Assert.IsAssignableFrom<QueuedItemResult<SchedulerSuccess>>(result);
            successResult.Result.AddedPaths.Should().BeEquivalentTo(request.Paths);
        }

        [Theory]
        [SchedulerRequestProcessorAutoData(respectsRobotsTxt: true, allowedByRobots: false)]
        public async Task ProcessItemShouldNotQueuePathsDisallowedByRobotsTxt(
            [Frozen] SchedulerRequest request,
            SchedulerRequestProcessor sut)
        {
            var result = await sut.ProcessItemAsync(request);
            var successResult = Assert.IsAssignableFrom<QueuedItemResult<SchedulerSuccess>>(result);
            Assert.Empty(successResult.Result.AddedPaths);
        }

        [Theory]
        [SchedulerRequestProcessorAutoData(respectsRobotsTxt: true, robotsContentExists: false)]
        public async Task ProcessItemShouldReturnWaitingIfRobotsTxtNotCached(
            [Frozen] SchedulerRequest request,
            SchedulerRequestProcessor sut)
        {
            var result = await sut.ProcessItemAsync(request);
            Assert.Equal(QueuedItemStatus.Waiting, result.Status);
        }

        [Theory]
        [SchedulerRequestProcessorAutoData(uri: "http://non-maching.url", includeDomains: new[] { "*.non-matching-pattern" })]
        public async Task ProcessItemShouldNotQueuePathsForDomainNotInIncludeList(
            [Frozen] SchedulerRequest request,
            SchedulerRequestProcessor sut)
        {
            var result = await sut.ProcessItemAsync(request);
            var success = Assert.IsAssignableFrom<QueuedItemResult<SchedulerSuccess>>(result);
            Assert.Empty(success.Result.AddedPaths);
        }

        [Theory]
        [SchedulerRequestProcessorAutoData(uri: "http://maching.url", includeDomains: new[] { "*.url" })]
        public async Task ProcessItemShouldQueueAllPathsForDomainInIncludeList(
            [Frozen] SchedulerRequest request,
            SchedulerRequestProcessor sut)
        {
            var result = await sut.ProcessItemAsync(request);
            var success = Assert.IsAssignableFrom<QueuedItemResult<SchedulerSuccess>>(result);
            success.Result.AddedPaths.Should().BeEquivalentTo(request.Paths);
        }

        [Theory]
        [SchedulerRequestProcessorAutoData(uri: "http://matching.url", excludeDomains: new[] { "*.url" })]
        public async Task ProcessItemShouldNotQueuePathsForDomainInExcludeList(
            [Frozen] SchedulerRequest request,
            SchedulerRequestProcessor sut)
        {
            var result = await sut.ProcessItemAsync(request);
            var success = Assert.IsAssignableFrom<QueuedItemResult<SchedulerSuccess>>(result);
            Assert.Empty(success.Result.AddedPaths);
        }

        [Theory]
        [SchedulerRequestProcessorAutoData(uri: "http://maching.url", excludeDomains: new[] { "*.non-matching-pattern" })]
        public async Task ProcessItemShouldQueueAllPathsForDomainNotInExcludeList(
            [Frozen] SchedulerRequest request,
            SchedulerRequestProcessor sut)
        {
            var result = await sut.ProcessItemAsync(request);
            var success = Assert.IsAssignableFrom<QueuedItemResult<SchedulerSuccess>>(result);
            success.Result.AddedPaths.Should().BeEquivalentTo(request.Paths);
        }
    }
}
