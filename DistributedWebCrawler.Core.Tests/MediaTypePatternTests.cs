using Xunit;

namespace DistributedWebCrawler.Core.Tests
{
    public class MediaTypePatternTests
    {
        [Theory]
        [InlineData("text/html")]
        [InlineData("text/*")]
        [InlineData("*/html")]
        [InlineData("*/*")]
        public void TryCreateShouldSucceedForValidMediaType(string validMediaType)
        {
            var success = MediaTypePattern.TryCreate(validMediaType, out var result);
            Assert.True(success);
            Assert.NotNull(result);
        }

        [Theory]
        [InlineData("")]
        [InlineData("text/")]
        [InlineData("text/*/")]
        [InlineData("/")]
        [InlineData("*")]
        public void TryCreateShouldFailForInvalidMediaType(string invalidMediaType)
        {
            var failure = MediaTypePattern.TryCreate(invalidMediaType, out var result);
            Assert.False(failure);
            Assert.Null(result);
        }

        [Theory]
        [InlineData("*/*", "*/*")]
        [InlineData("*/*", "text/*")]
        [InlineData("*/*", "*/html")]
        [InlineData("*/*", "text/html")]
        [InlineData("text/*", "text/html")]
        [InlineData("*/html", "text/html")]
        [InlineData("text/html", "text/h*")]
        [InlineData("text/html", "text/h*l")]
        [InlineData("text/html", "text/*l")]
        [InlineData("application/json", "a*/*")]
        [InlineData("application/json", "a*/json")]
        [InlineData("application/json", "a*n/json")]
        [InlineData("application/json", "*n/json")]
        public void MatchShouldSucceedForMatchingPattern(string mediaType, string patternToMatch)
        {
            TestMediaTypeMatch(mediaType, patternToMatch, expectedResult: true);
        }

        [Theory]
        [InlineData("text/html", "text/htm")]
        [InlineData("text/html", "*n/html")]
        [InlineData("text/html", "tex*n/html")]
        public void MatchShouldFailForNonMatchingPattern(string mediaType, string patternToMatch)
        {
            TestMediaTypeMatch(mediaType, patternToMatch, expectedResult: false);
        }

        private static void TestMediaTypeMatch(string patternString, string otherString, bool expectedResult)
        {
            MediaTypePattern.TryCreate(patternString, out var pattern);
            Assert.NotNull(pattern);

            MediaTypePattern.TryCreate(otherString, out var other);
            
            Assert.NotNull(other);
            Assert.Equal(expectedResult, pattern!.Match(other!));
            Assert.Equal(expectedResult, other!.Match(pattern));
        }
    }
}