using DistributedWebCrawler.Core.Tests.Customizations;

namespace DistributedWebCrawler.Core.Tests.Attributes
{
    internal class LinkParserAutoDataAttribute : MoqInlineAutoDataAttribute
    {
        public LinkParserAutoDataAttribute(string content, string[] expectedResults)
            : base(new AngleSharpLinkParserCustomization(), values: new object[] { content, expectedResults })
        {

        }
    }
}
