using AngleSharp.Html.Parser;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DistributedWebCrawler.Core.LinkParser
{
    public class AngleSharpLinkParser : LinkParserBase
    {
        private readonly IHtmlParser _htmlParser;
        public AngleSharpLinkParser(IHtmlParser htmlParser)
        {
            _htmlParser = htmlParser;
        }

        protected override async Task<IEnumerable<Hyperlink>> ParseLinksAsync(string content)
        {
            var document = await _htmlParser.ParseDocumentAsync(content).ConfigureAwait(false);

            var hyperlinks = document.QuerySelectorAll("a")               
                .Select(e => {
                    var linkText = (e.GetAttribute("href") ?? string.Empty).Trim().TrimEnd('#');
                    if (linkText.EndsWith("#/"))
                    {
                        linkText = linkText.Substring(0, linkText.Length - 2) + '/';
                    }
                    return linkText;
                })
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Distinct()
                .Select(text => new Hyperlink {  Href = text });

            return hyperlinks;
        }
    }
}
