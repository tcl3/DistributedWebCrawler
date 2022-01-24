using AutoFixture;
using DistributedWebCrawler.Core.LinkParser;
using System.Linq;

namespace DistributedWebCrawler.Core.Tests.Customizations
{
    internal class ParserRequestProcessorCustomization : ICustomization
    {
        private readonly string[]? _hyperlinks;

        public ParserRequestProcessorCustomization(string[]? hyperlinks = null)
        {
            _hyperlinks = hyperlinks;
        }

        public void Customize(IFixture fixture)
        {
            if (_hyperlinks != null)
            {
                var hyperlinks = _hyperlinks.Select(href => new Hyperlink { Href = href });
                fixture.Inject(hyperlinks);
            }
        }
    }
}
