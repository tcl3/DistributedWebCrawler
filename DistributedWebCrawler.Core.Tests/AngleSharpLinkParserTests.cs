using DistributedWebCrawler.Core.LinkParser;
using DistributedWebCrawler.Core.Tests.Attributes;
using FluentAssertions;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace DistributedWebCrawler.Core.Tests
{
    public class AngleSharpLinkParserTests
    {
        [Theory]
        [LinkParserAutoData("", expectedResults: new string[] { })]
        public async Task NoLinksReturnedWhenNoContentGiven(string content, string[] expectedResults, AngleSharpLinkParser sut)
        {
            await ParseTest(content, expectedResults, sut);
        }

        [Theory]
        [LinkParserAutoData("<a href=\"http://test.com/\"></a>", expectedResults: new[] { "http://test.com/" })]
        public async Task SingleLinkReturnedWhenSingleLinkTagParsed(string content, string[] expectedResults, AngleSharpLinkParser sut)
        {
            await ParseTest(content, expectedResults, sut);
        }

        [Theory]
        [LinkParserAutoData("<a href=\"http://test.com/\"></a><a href=\"http://test2.com/\"></a>", expectedResults: new[] { "http://test.com/", "http://test2.com/" })]
        public async Task MultipleLinksReturnedWhenMultipleLinkTagsParsed(string content, string[] expectedResults, AngleSharpLinkParser sut)
        {
            await ParseTest(content, expectedResults, sut);
        }

        [Theory]
        [LinkParserAutoData("<a href=\"http://test.com#/\"></a>", expectedResults: new[] { "http://test.com/" })]
        [LinkParserAutoData("<a href=\"http://test.com/#\"></a>", expectedResults: new[] { "http://test.com/" })]
        public async Task EnsureEmptyFragmentIsRemovedFromUri(string content, string[] expectedResults, AngleSharpLinkParser sut)
        {
            await ParseTest(content, expectedResults, sut);
        }

        [Theory]
        [LinkParserAutoData("<a href=\"http://test.com#fragment/\"></a>", expectedResults: new[] { "http://test.com#fragment/" })]
        [LinkParserAutoData("<a href=\"http://test.com/#fragment\"></a>", expectedResults: new[] { "http://test.com/#fragment" })]
        public async Task EnsureNonEmptyFragmentIsRetainedInUri(string content, string[] expectedResults, AngleSharpLinkParser sut)
        {
            await ParseTest(content, expectedResults, sut);
        }

        [Theory]
        [LinkParserAutoData("<a href=\"http://test.com/     \"></a>", expectedResults: new[] { "http://test.com/" })]
        [LinkParserAutoData("<a href=\"  http://test.com/\"></a>", expectedResults: new[] { "http://test.com/" })]
        [LinkParserAutoData("<a href=\"  http://test.com/   \"></a>", expectedResults: new[] { "http://test.com/" })]
        public async Task EnsureSurroundingWhitespaceIsTrimmed(string content, string[] expectedResults, AngleSharpLinkParser sut)
        {
            await ParseTest(content, expectedResults, sut);
        }

        private async Task ParseTest(string content, string[] expectedResults, AngleSharpLinkParser sut)
        {
            var hyperlinks = await sut.ParseLinksAsync(content);

            var hrefs = hyperlinks.Select(x => x.Href);

            hrefs.Should().BeEquivalentTo(expectedResults);
        }
    }
}
