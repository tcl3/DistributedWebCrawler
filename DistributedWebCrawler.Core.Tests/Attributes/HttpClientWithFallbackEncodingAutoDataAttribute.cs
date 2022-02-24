using AutoFixture;
using AutoFixture.Xunit2;
using DistributedWebCrawler.Core.Tests.Customizations;
using System;

namespace DistributedWebCrawler.Core.Tests.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    internal class HttpClientWithFallbackEncodingAutoDataAttribute : AutoDataAttribute
    {
        public HttpClientWithFallbackEncodingAutoDataAttribute(string? responseCharSet, string? fallbackEncoding = null)
            : base(() => GetCustomizations(responseCharSet, fallbackEncoding))
        {

        }

        private static IFixture GetCustomizations(string? responseCharSet, string? fallbackEncoding)
        {
            var fixture = new Fixture();
            if (fallbackEncoding != null)
            {
                fixture.Customize(new EncodingCustomization(fallbackEncoding));
            }

            fixture.Customize(new HttpResponseMessageCharsetCustomization(responseCharSet))
                   .Customize(new HttpClientCustomization<FallbackEncodingHandler>());

            return fixture;
        }
    }
}
