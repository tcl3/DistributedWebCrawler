using DistributedWebCrawler.Core.Tests.Customizations;

namespace DistributedWebCrawler.Core.Tests.Attributes
{
    internal class HttpClientAutoDataAttribute : MoqAutoDataAttribute
    {
        public HttpClientAutoDataAttribute() : base(new HttpClientCustomization())
        {

        }
    }
}
