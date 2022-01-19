using AutoFixture;
using AutoFixture.Kernel;
using System.Net.Http;

namespace DistributedWebCrawler.Core.Tests.Customizations
{
    internal class HttpResponseMessageCharsetCustomization : ICustomization
    {
        private readonly string? _charset;

        public HttpResponseMessageCharsetCustomization(string? charset)
        {
            _charset = charset;
        }
        public void Customize(IFixture fixture)
        {
            fixture.Customizations.Add(new TypeRelay(
                typeof(HttpContent),
                typeof(StringContent)
            ));

            fixture.Customize<StringContent>(c => c
                .Do(x => x.Headers.ContentType!.CharSet = _charset)
            );
        }
    }
}
