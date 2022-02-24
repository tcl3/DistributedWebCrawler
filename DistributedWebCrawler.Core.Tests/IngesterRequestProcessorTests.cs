using AutoFixture.Xunit2;
using DistributedWebCrawler.Core.Interfaces;
using DistributedWebCrawler.Core.Models;
using DistributedWebCrawler.Core.RequestProcessors;
using DistributedWebCrawler.Core.Tests.Attributes;
using DistributedWebCrawler.Core.Tests.Collections;
using Moq;
using System;
using System.Linq;
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
        [IngesterAutoData(includeMediaTypes: new[] { "application/json", "text/html" }, contentTypeHeaderValue: "text/html", content: "dummy")]
        public async Task ProcessItemShouldReturnSuccessWhenContentMediaTypeIsInAllowedList(
            [Frozen] Mock<IProducer<ParseRequest>> parseRequestProducerMock,
            [Frozen] IngestRequest request,
            IngesterRequestProcessor sut)
        {
            var result = await sut.ProcessItemAsync(request);

            var success = Assert.IsAssignableFrom<QueuedItemResult<IngestSuccess>>(result);
            parseRequestProducerMock.Verify(x => x.Enqueue(It.IsAny<ParseRequest>()), Times.Once());
            Assert.Equal("text/html", success.Result.MediaType);
        }

        [Theory]
        [IngesterAutoData(contentTypeHeaderValue: null, content: "dummy")]
        public async Task ProcessItemShouldNotProduceParseRequestWhenMediaTypeHeaderIsNotPresent(
            [Frozen] Mock<IProducer<ParseRequest>> parseRequestProducerMock,
            [Frozen] IngestRequest request,
            IngesterRequestProcessor sut)
        {
            var result = await sut.ProcessItemAsync(request);

            var success = Assert.IsAssignableFrom<QueuedItemResult<IngestSuccess>>(result);
            parseRequestProducerMock.Verify(x => x.Enqueue(It.IsAny<ParseRequest>()), Times.Never());
            Assert.Null(success.Result.MediaType);
        }

        [Theory]
        [IngesterAutoData(includeMediaTypes: new[] { "text/html" }, contentTypeHeaderValue: "invalidmediatypeheader", content: "dummy")]
        [IngesterAutoData(includeMediaTypes: new[] { "application/json", "text/html" }, contentTypeHeaderValue: "invalidmediatypeheader", content: "dummy")]
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
        [IngesterAutoData(excludeMediaTypes: new[] { "text/html" }, content: "dummy", contentTypeHeaderValue: "text/plain")]
        [IngesterAutoData(excludeMediaTypes: new[] { "application/json", "text/html" }, content: "dummy", contentTypeHeaderValue: "text/plain")]
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
        [IngesterAutoData(statusCode: HttpStatusCode.BadRequest)]
        [IngesterAutoData(statusCode: HttpStatusCode.InternalServerError)]
        [IngesterAutoData(statusCode: HttpStatusCode.BadRequest, locationHeaderValue: "http://should-not-be-followed.xxx/")]
        public async Task ProcessItemShouldReturnFailureWhenHttpResponseCodeIndicatesError(
            [Frozen] Mock<IProducer<ParseRequest>> parseRequestProducerMock,
            [Frozen] IngestRequest request,
            IngesterRequestProcessor sut)
        {
            var result = await sut.ProcessItemAsync(request);

            var failure = Assert.IsAssignableFrom<QueuedItemResult<IngestFailure>>(result);
            Assert.Equal(IngestFailureReason.Http4xxError, failure.Result.Error);
            Assert.Equal(request.Uri, failure.Result.Uri);
            Assert.Empty(failure.Result.Redirects);

            parseRequestProducerMock.Verify(x => x.Enqueue(It.IsAny<ParseRequest>()), Times.Never());
        }

        [Theory]
        [IngesterAutoData(statusCode: HttpStatusCode.MultipleChoices, locationHeaderValue: "http://test.test", maxRedirects: 5)]
        [IngesterAutoData(statusCode: HttpStatusCode.Redirect, locationHeaderValue: "http://test.test", maxRedirects: 5)]
        [IngesterAutoData(statusCode: HttpStatusCode.MovedPermanently, locationHeaderValue: "http://test.test", maxRedirects: 5)]
        [IngesterAutoData(statusCode: (HttpStatusCode)399, locationHeaderValue: "http://test.test", maxRedirects: 5)]
        public async Task ProcessItemShouldReturnFailureWhenMaxRedirectsReachedWithAbsoluteUriLocationHeader (
            [Frozen] Mock<IProducer<ParseRequest>> parseRequestProducerMock,
            [Frozen] IngestRequest request,
            IngesterRequestProcessor sut)
        {
            var result = await sut.ProcessItemAsync(request);

            var failure = Assert.IsAssignableFrom<QueuedItemResult<IngestFailure>>(result);
            Assert.Equal(IngestFailureReason.MaxRedirectsReached, failure.Result.Error);

            var redirectResults = failure.Result.Redirects
                .Select(x => x.DestinationUri)
                .ToList();

            Assert.Equal(redirectResults, Enumerable.Repeat(new Uri("http://test.test/"), 5).ToList());

            parseRequestProducerMock.Verify(x => x.Enqueue(It.IsAny<ParseRequest>()), Times.Never());
            
        }

        [Theory]
        [IngesterAutoData(statusCode: HttpStatusCode.MultipleChoices, locationHeaderValue: "/relative-uri-redirect", maxRedirects: 5)]
        [IngesterAutoData(statusCode: HttpStatusCode.Redirect, locationHeaderValue: "/relative-uri-redirect", maxRedirects: 5)]
        [IngesterAutoData(statusCode: HttpStatusCode.MovedPermanently, locationHeaderValue: "/relative-uri-redirect", maxRedirects: 5)]
        [IngesterAutoData(statusCode: (HttpStatusCode)399, locationHeaderValue: "/relative-uri-redirect", maxRedirects: 5)]
        public async Task ProcessItemShouldReturnFailureWhenMaxRedirectsReachedWithRelativeUriLocationHeader(
            [Frozen] Mock<IProducer<ParseRequest>> parseRequestProducerMock,
            [Frozen] IngestRequest request,
            IngesterRequestProcessor sut)
        {
            var result = await sut.ProcessItemAsync(request);

            var failure = Assert.IsAssignableFrom<QueuedItemResult<IngestFailure>>(result);
            Assert.Equal(IngestFailureReason.MaxRedirectsReached, failure.Result.Error);

            var redirectResults = failure.Result.Redirects
                .Select(x => x.DestinationUri)
                .ToList();

            Assert.Equal(redirectResults, Enumerable.Repeat(new Uri(request.Uri, "/relative-uri-redirect"), 5).ToList());

            parseRequestProducerMock.Verify(x => x.Enqueue(It.IsAny<ParseRequest>()), Times.Never());

        }

        [Theory]
        [IngesterAutoData(statusCode: HttpStatusCode.OK, locationHeaderValue: "http://test.test")]
        [IngesterAutoData(statusCode: (HttpStatusCode)299, locationHeaderValue: "http://test.test")]
        
        public async Task ProcessItemShouldIgnoreLocationHeaderIfStatusCodeNot3xx(
            [Frozen] Mock<IProducer<ParseRequest>> parseRequestProducerMock,
            [Frozen] IngestRequest request,
            IngesterRequestProcessor sut)
        {
            var result = await sut.ProcessItemAsync(request);

            var success = Assert.IsAssignableFrom<QueuedItemResult<IngestSuccess>>(result);

            var redirectResults = success.Result.Redirects
                .Select(x => x.DestinationUri)
                .ToList();

            Assert.Empty(redirectResults);

            parseRequestProducerMock.Verify(x => x.Enqueue(It.IsAny<ParseRequest>()), Times.Once());


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
        [IngesterAutoData(maxContentLengthBytes: 2, content: "hi")]
        [IngesterAutoData(maxContentLengthBytes: 3, content: "hi")]
        public async Task ProcessItemShouldReturnSuccessWhenContentSmallerOrEqualToMaxAllowedContentLength(
            [Frozen] Mock<IProducer<ParseRequest>> parseRequestProducerMock,
            [Frozen] IngestRequest request,
            IngesterRequestProcessor sut)
        {
            var result = await sut.ProcessItemAsync(request);

            var success = Assert.IsAssignableFrom<QueuedItemResult<IngestSuccess>>(result);
            parseRequestProducerMock.Verify(x => x.Enqueue(It.IsAny<ParseRequest>()), Times.Once());
        }

        [Theory]
        [IngesterAutoData(includeMediaTypes: new[] { "text/html" }, contentTypeHeaderValue: "application/json")]
        [IngesterAutoData(includeMediaTypes: new[] { "text/html", "application/gzip" }, contentTypeHeaderValue: "application/json")]
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
        [IngesterAutoData(excludeMediaTypes: new[] { "text/plain", "text/html" }, contentTypeHeaderValue: "text/html")]
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
