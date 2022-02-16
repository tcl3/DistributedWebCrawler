using AutoFixture.Xunit2;
using DistributedWebCrawler.Core.Interfaces;
using DistributedWebCrawler.Core.Models;
using DistributedWebCrawler.Core.RequestProcessors;
using DistributedWebCrawler.Core.Tests.Attributes;
using DistributedWebCrawler.Core.Tests.Collections;
using Moq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace DistributedWebCrawler.Core.Tests
{
    [Collection(nameof(RequestProcessorCollection))]
    public class ParserRequestProcessorTests
    {
        [Theory]
        [ParserAutoData]
        public async Task ProcessItemShouldReturnFailureIfNoItemInContentStore(
            [Frozen] Mock<IContentStore> contentStoreMock,
            [Frozen] ParseRequest request,
            ParserRequestProcessor sut)
        {
            contentStoreMock.Setup(x => x.GetContentAsync(request.ContentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => string.Empty);

            var result = await sut.ProcessItemAsync(request);

            var failure = Assert.IsAssignableFrom<QueuedItemResult<ErrorCode<ParseFailure>>>(result);
            Assert.Equal(ParseFailure.NoItemInContentStore, failure.Result.Error);
        }

        [Theory]
        [ParserAutoData(hyperlinks: new string[] { })]
        public async Task ProcessItemShouldReturnFailureIfNoLinksFound(
            [Frozen] Mock<IProducer<SchedulerRequest>> requestProducerMock,
            [Frozen] ParseRequest request,
            ParserRequestProcessor sut)
        {
            var result = await sut.ProcessItemAsync(request);

            var failure = Assert.IsAssignableFrom<QueuedItemResult<ErrorCode<ParseFailure>>>(result);

            Assert.Equal(ParseFailure.NoLinksFound, failure.Result.Error);
            requestProducerMock.Verify(x => x.Enqueue(It.IsAny<SchedulerRequest>()), Times.Never());
        }

        [Theory]
        [ParserAutoData(hyperlinks: new string[] { "http://absolute-uri.com" })]
        public async Task ProcessItemShouldSendSchedulerRequestIfValidAbsoluteUriFound(
            [Frozen] Mock<IProducer<SchedulerRequest>> requestProducerMock,
            [Frozen] ParseRequest request,
            ParserRequestProcessor sut)
        {
            var result = await sut.ProcessItemAsync(request);

            var success = Assert.IsAssignableFrom<QueuedItemResult<ParseSuccess>>(result);            
            requestProducerMock.Verify(x => x.Enqueue(It.IsAny<SchedulerRequest>()), Times.Once());
        }

        [Theory]
        [ParserAutoData(hyperlinks: new string[] { "http://invalid uri" })]
        public async Task ProcessItemShouldSendNotSchedulerRequestIfInvalidUriFound(
            [Frozen] Mock<IProducer<SchedulerRequest>> requestProducerMock,
            [Frozen] ParseRequest request,
            ParserRequestProcessor sut)
        {
            var result = await sut.ProcessItemAsync(request);

            var success = Assert.IsAssignableFrom<QueuedItemResult<ParseSuccess>>(result);
            requestProducerMock.Verify(x => x.Enqueue(It.IsAny<SchedulerRequest>()), Times.Never());
        }

        [Theory]
        [ParserAutoData(hyperlinks: new string[] { "/relative-path" })]
        [ParserAutoData(hyperlinks: new string[] { "relative-path" })]
        public async Task ProcessItemShouldSendSchedulerRequestIfValidRelativeUriFound(
            [Frozen] Mock<IProducer<SchedulerRequest>> requestProducerMock,
            [Frozen] ParseRequest request,
            ParserRequestProcessor sut)
        {
            var result = await sut.ProcessItemAsync(request);

            var success = Assert.IsAssignableFrom<QueuedItemResult<ParseSuccess>>(result);            
            requestProducerMock.Verify(x => x.Enqueue(It.IsAny<SchedulerRequest>()), Times.Once());
        }

        [Theory]
        [ParserAutoData(hyperlinks: new string[] { "tel://911" })]
        [ParserAutoData(hyperlinks: new string[] { "mailto://test@test.test"})]
        [ParserAutoData(hyperlinks: new string[] { "invalidscheme://test.test" })]
        public async Task ProcessItemShouldSendNotSchedulerRequestForUriWithNonHttpScheme(
            [Frozen] Mock<IProducer<SchedulerRequest>> requestProducerMock,
            [Frozen] ParseRequest request,
            ParserRequestProcessor sut)
        {
            var result = await sut.ProcessItemAsync(request);

            var success = Assert.IsAssignableFrom<QueuedItemResult<ParseSuccess>>(result);
            requestProducerMock.Verify(x => x.Enqueue(It.IsAny<SchedulerRequest>()), Times.Never());
        }
    }
}