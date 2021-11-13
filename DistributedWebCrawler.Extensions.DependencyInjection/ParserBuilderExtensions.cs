using AngleSharp.Html.Parser;
using DistributedWebCrawler.Core.LinkParser;
using DistributedWebCrawler.Extensions.DependencyInjection.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace DistributedWebCrawler.Extensions.DependencyInjection
{
    public static class ParserBuilderExtensions
    {
        public static IParserBuilder WithAngleSharpLinkParser(this IParserBuilder parserBuilder)
        {
            parserBuilder.Services.AddSingleton<IHtmlParser>(s => new HtmlParser());
            return parserBuilder.WithLinkParser<AngleSharpLinkParser>();
        }
    }
}
