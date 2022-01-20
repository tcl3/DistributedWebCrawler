using AngleSharp.Html.Parser;
using AutoFixture;
using AutoFixture.Kernel;

namespace DistributedWebCrawler.Core.Tests.Customizations
{
    internal class AngleSharpLinkParserCustomization : ICustomization
    {
        public void Customize(IFixture fixture)
        {
            // FIXME: would be better to mock this properly rather than relying on a type relay
            fixture.Customizations.Add(new TypeRelay(
                typeof(IHtmlParser),
                typeof(HtmlParser)));
        }
    }
}
