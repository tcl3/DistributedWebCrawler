using AutoFixture;
using DistributedWebCrawler.Core.Tests.Customizations;
using System.Net;

namespace DistributedWebCrawler.Core.Tests.Attributes
{
    internal class IngesterAutoDataAttribute : MoqInlineAutoDataAttribute
    {

        public IngesterAutoDataAttribute(
            bool maxDepthReached = false,
            HttpStatusCode statusCode = HttpStatusCode.OK,
            string? locationHeaderValue = null,
            string? contentTypeHeaderValue = "text/plain",
            string content = "",
            string[]? includeMediaTypes = null,
            string[]? excludeMediaTypes = null,
            int maxContentLengthBytes = -1,
            int maxConcurrentItems = 1,
            int maxRedirects = 1
        )
            : base(new ICustomization[] 
            { 
                new IngesterRequestProcessorCustomization(
                    maxDepthReached, 
                    includeMediaTypes, 
                    excludeMediaTypes,
                    maxContentLengthBytes >= 0 ? maxContentLengthBytes : null, 
                    maxConcurrentItems, 
                    maxRedirects),
                
                new HttpClientCustomization(
                    statusCode, 
                    content, 
                    locationHeaderValue,
                    contentTypeHeaderValue),
            }, configureMembers: true)
        {
        }
    }

    internal class ExceptionThrowingIngesterAutoDataAttribute : MoqAutoDataAttribute
    {
        public ExceptionThrowingIngesterAutoDataAttribute(
            string? exceptionMessage = null,
            int maxConcurrentItems = 1
        )
            : base(new ICustomization[]
            {
                new IngesterRequestProcessorCustomization(maxConcurrentItems: maxConcurrentItems),
                new FakeHttpMessageHandlerCustomization(exceptionMessage),
                new HttpClientCustomization(),
            }, configureMembers: true)
        {
        }
    }

    internal class CancelledIngesterAutoDataAttribute : MoqAutoDataAttribute
    {
        public CancelledIngesterAutoDataAttribute(int maxConcurrentItems = 1)
            : base(new ICustomization[]
            {
                new IngesterRequestProcessorCustomization(maxConcurrentItems: maxConcurrentItems),
                new FakeHttpMessageHandlerCustomization(isCancelled: true),
                new HttpClientCustomization(),
            }, configureMembers: true)
        {
        }
    }
}
