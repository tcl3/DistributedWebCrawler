using AutoFixture;
using AutoFixture.Xunit2;
using DistributedWebCrawler.Core.Tests.Customizations;

namespace DistributedWebCrawler.Core.Tests.Attributes
{
    internal class HttpClientWithFallbackEncodingAutoDataAttribute : AutoDataAttribute
    {
        public HttpClientWithFallbackEncodingAutoDataAttribute(string? charset)
            :base(() => new Fixture()
                .Customize(new HttpResponseMessageCharsetCustomization(charset))
                .Customize(new HttpClientCustomization<FallbackEncodingHandler>()))
        {

        }
    }
}
