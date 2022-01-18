using DistributedWebCrawler.Core.Tests.Customizations;
using System.Net;

namespace DistributedWebCrawler.Core.Tests.Attributes
{
    internal class HttpClientAutoDataAttribute : MoqAutoDataAttribute
    {
        public HttpClientAutoDataAttribute(
            HttpStatusCode statusCode = HttpStatusCode.OK, 
            string content = "",
            string? locationHeaderValue = null,
            string? contentTypeHeaderValue = null) 
            : base(new HttpClientCustomization(statusCode, content, locationHeaderValue, contentTypeHeaderValue))
        {

        }
    }

    internal class ExceptionThrowingHttpClientAutoDataAttribute : MoqAutoDataAttribute
    {
        public ExceptionThrowingHttpClientAutoDataAttribute(string? exceptionMessage = null)
            : base(new HttpClientCustomization(exceptionMessage))
        {

        }
    }

    internal class CancelledHttpClientAutoDataAttribute : MoqAutoDataAttribute
    {
        public CancelledHttpClientAutoDataAttribute() : base(new CancelledHttpClientCustomization())
        {

        }
    }
}
