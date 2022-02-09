using AutoFixture.Xunit2;
using DistributedWebCrawler.Core.Interfaces;
using DistributedWebCrawler.Core.Models;
using DistributedWebCrawler.Core.RequestProcessors;
using DistributedWebCrawler.Core.Tests.Attributes;
using DistributedWebCrawler.Core.Tests.Collections;
using Moq;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace DistributedWebCrawler.Core.Tests
{
    [Collection(nameof(RequestProcessorCollection))]

    public class IngesterRequestProcessorTests
    {
        [Theory]
        [IngesterAutoData(maxDepthReached: true)]
        public async Task ProcessItemShouldReturnFailureWhenMaxDepthReached(
            [Frozen] Mock<IProducer<ParseRequest>> parseRequestProducerMock,
            [Frozen] IngestRequest request,
            IngesterRequestProcessor sut)
        {
            var result = await sut.ProcessItemAsync(request);

            var failure = Assert.IsAssignableFrom<QueuedItemResult<IngestFailure>>(result);
            Assert.Equal(IngestFailureReason.MaxDepthReached, failure.Result.Error);

            parseRequestProducerMock.Verify(x => x.Enqueue(It.IsAny<ParseRequest>()), Times.Never());
            
        }

        [Theory]
        [IngesterAutoData(content: "dummy")]
        public async Task ProcessItemShouldReturnSuccessWhenValidContentRetrieved(
            [Frozen] Mock<IProducer<ParseRequest>> parseRequestProducerMock,
            [Frozen] IngestRequest request,
            IngesterRequestProcessor sut)
        {
            var result = await sut.ProcessItemAsync(request);

            var success = Assert.IsAssignableFrom<QueuedItemResult<IngestSuccess>>(result);
            parseRequestProducerMock.Verify(x => x.Enqueue(It.IsAny<ParseRequest>()), Times.Once());
        }

        [Theory]
        [IngesterAutoData(includeMediaTypes: new[] { "text/html" }, contentTypeHeaderValue: "text/html", content: "dummy")]
        public async Task ProcessItemShouldReturnSuccessWhenContentMediaTypeIsInAllowedList(
            [Frozen] Mock<IProducer<ParseRequest>> parseRequestProducerMock,
            [Frozen] IngestRequest request,
            IngesterRequestProcessor sut)
        {
            var result = await sut.ProcessItemAsync(request);

            var success = Assert.IsAssignableFrom<QueuedItemResult<IngestSuccess>>(result);
            parseRequestProducerMock.Verify(x => x.Enqueue(It.IsAny<ParseRequest>()), Times.Once());
        }

        [Theory]
        [IngesterAutoData(includeMediaTypes: new[] { "text/html" }, contentTypeHeaderValue: "invalidmediatypeheader", content: "dummy")]
        public async Task ProcessItemShouldReturnSuccessWhenContentMediaTypeHeaderIsInvalid(
            [Frozen] Mock<IProducer<ParseRequest>> parseRequestProducerMock,
            [Frozen] IngestRequest request,
            IngesterRequestProcessor sut)
        {
            var result = await sut.ProcessItemAsync(request);

            var success = Assert.IsAssignableFrom<QueuedItemResult<IngestSuccess>>(result);
            
            // Currently only requests with text/html and text/plain content types are sent to the parser
            parseRequestProducerMock.Verify(x => x.Enqueue(It.IsAny<ParseRequest>()), Times.Never());
        }

        [Theory]
        [IngesterAutoData(excludeMediaTypes: new[] { "text/html" }, content: "dummy")]
        public async Task ProcessItemShouldReturnSuccessWhenContentMediaTypeNotInExcludeList(
            [Frozen] Mock<IProducer<ParseRequest>> parseRequestProducerMock,
            [Frozen] IngestRequest request,
            IngesterRequestProcessor sut)
        {
            var result = await sut.ProcessItemAsync(request);

            var success = Assert.IsAssignableFrom<QueuedItemResult<IngestSuccess>>(result);
            parseRequestProducerMock.Verify(x => x.Enqueue(It.IsAny<ParseRequest>()), Times.Once());
        }

        [Theory]
        [IngesterAutoData(statusCode: HttpStatusCode.NotFound)]
        public async Task ProcessItemShouldReturnFailureWhenHttpResponseCodeIndicatesError(
            [Frozen] Mock<IProducer<ParseRequest>> parseRequestProducerMock,
            [Frozen] IngestRequest request,
            IngesterRequestProcessor sut)
        {
            var result = await sut.ProcessItemAsync(request);

            var failure = Assert.IsAssignableFrom<QueuedItemResult<IngestFailure>>(result);
            Assert.Equal(IngestFailureReason.Http4xxError, failure.Result.Error);

            parseRequestProducerMock.Verify(x => x.Enqueue(It.IsAny<ParseRequest>()), Times.Never());
        }

        [Theory]
        [IngesterAutoData(statusCode: HttpStatusCode.Redirect, locationHeaderValue: "http://test.test", maxRedirects: 5)]
        public async Task ProcessItemShouldReturnFailureWhenMaxRedirectsReached (
            [Frozen] Mock<IProducer<ParseRequest>> parseRequestProducerMock,
            [Frozen] IngestRequest request,
            IngesterRequestProcessor sut)
        {
            var result = await sut.ProcessItemAsync(request);

            var failure = Assert.IsAssignableFrom<QueuedItemResult<IngestFailure>>(result);
            Assert.Equal(IngestFailureReason.MaxRedirectsReached, failure.Result.Error);

            parseRequestProducerMock.Verify(x => x.Enqueue(It.IsAny<ParseRequest>()), Times.Never());
            
        }

        [Theory]
        [IngesterAutoData(maxContentLengthBytes: 1, content: "hi")]
        public async Task ProcessItemShouldReturnFailureWhenContentLargerThanMaxAllowedContentLength(
            [Frozen] Mock<IProducer<ParseRequest>> parseRequestProducerMock,
            [Frozen] IngestRequest request,
            IngesterRequestProcessor sut)
        {
            var result = await sut.ProcessItemAsync(request);

            var failure = Assert.IsAssignableFrom<QueuedItemResult<IngestFailure>>(result);
            Assert.Equal(IngestFailureReason.ContentTooLarge, failure.Result.Error);

            parseRequestProducerMock.Verify(x => x.Enqueue(It.IsAny<ParseRequest>()), Times.Never());
        }

        [Theory]
        [IngesterAutoData(includeMediaTypes: new[] { "text/html" }, contentTypeHeaderValue: "application/json")]
        public async Task ProcessItemShouldReturnFailureWhenMediaTypeNotInAllowedList(
            [Frozen] Mock<IProducer<ParseRequest>> parseRequestProducerMock,
            [Frozen] IngestRequest request,
            IngesterRequestProcessor sut)
        {
            var result = await sut.ProcessItemAsync(request);

            var failure = Assert.IsAssignableFrom<QueuedItemResult<IngestFailure>>(result);
            Assert.Equal(IngestFailureReason.MediaTypeNotPermitted, failure.Result.Error);

            parseRequestProducerMock.Verify(x => x.Enqueue(It.IsAny<ParseRequest>()), Times.Never());
            
        }

        [Theory]
        [IngesterAutoData(excludeMediaTypes: new[] { "text/html" }, contentTypeHeaderValue: "text/html")]
        public async Task ProcessItemShouldReturnFailureWhenMediaTypeInExcludeList(
            [Frozen] Mock<IProducer<ParseRequest>> parseRequestProducerMock,
            [Frozen] IngestRequest request,
            IngesterRequestProcessor sut)
        {
            var result = await sut.ProcessItemAsync(request);

            var failure = Assert.IsAssignableFrom<QueuedItemResult<IngestFailure>>(result);
            Assert.Equal(IngestFailureReason.MediaTypeNotPermitted, failure.Result.Error);

            parseRequestProducerMock.Verify(x => x.Enqueue(It.IsAny<ParseRequest>()), Times.Never());
        }

        [Theory]
        [ExceptionThrowingIngesterAutoData(exceptionMessage: "this is a test")]
        public async Task ProcessItemShouldReturnFailureWhenExceptionThrownWhileRetrievingContent(
            [Frozen] Mock<IProducer<ParseRequest>> parseRequestProducerMock,
            [Frozen] IngestRequest request,
            IngesterRequestProcessor sut)
        {
            var result = await sut.ProcessItemAsync(request);

            var failure = Assert.IsAssignableFrom<QueuedItemResult<IngestFailure>>(result);
            Assert.Equal(IngestFailureReason.NetworkConnectivityError, failure.Result.Error);

            parseRequestProducerMock.Verify(x => x.Enqueue(It.IsAny<ParseRequest>()), Times.Never());
        }

        [Theory]
        [CancelledIngesterAutoData]
        public async Task ProcessItemShouldReturnFailureWhenRequestTimesOutWhileRetrievingContent(
            [Frozen] Mock<IProducer<ParseRequest>> parseRequestProducerMock,
            [Frozen] IngestRequest request,
            IngesterRequestProcessor sut)
        {
            var result = await sut.ProcessItemAsync(request);

            var failure = Assert.IsAssignableFrom<QueuedItemResult<IngestFailure>>(result);
            Assert.Equal(IngestFailureReason.RequestTimeout, failure.Result.Error);

            parseRequestProducerMock.Verify(x => x.Enqueue(It.IsAny<ParseRequest>()), Times.Never());
        }
    }
}
