using System;
using Xunit;

namespace DistributedWebCrawler.Core.Tests
{
    public class DomainPatternTests
    {
        [Fact]
        public void DomainPatternConstructorShouldFailForNullOrWhitespacePattern()
        {
            Assert.Throws<ArgumentNullException>(() => new DomainPattern(null!));
            Assert.Throws<ArgumentNullException>(() => new DomainPattern(""));
            Assert.Throws<ArgumentNullException>(() => new DomainPattern(" "));
        }

        [Fact]
        public void MatchShouldFailWhenPatternNullOrEmpty()
        {
            Assert.Throws<ArgumentNullException>(() => new DomainPattern("test").Match(null!));
            Assert.Throws<ArgumentNullException>(() => new DomainPattern("test").Match(""));
        }

        [Theory]
        [InlineData("test", "test")]
        [InlineData("test*", "test")]
        [InlineData("test**", "test")]
        [InlineData("tes*t", "tes123t")]
        [InlineData("*tes*t", "1tes123t")]
        [InlineData("*tes*t", "ttes123t")]
        [InlineData("*.com", "google.com")]
        [InlineData("*.google.com", "google.com")]
        [InlineData("*.google.com", "test.google.com")]
        [InlineData("*.google.com", "test.google.com.google.com")]
        public void MatchShouldReturnTrueForMatchingPattern(string matchingPattern, string domain) 
        {
            MatchTest(matchingPattern, domain, expectedResult: true);
        }

        [Theory]
        [InlineData("test", "test1")]
        [InlineData("test", "te")]
        [InlineData("t*est", "te")]
        [InlineData("t*est", "test1")]
        public void MatchShouldReturnFalseForNonMatchingPattern(string nonMatchingPattern, string domain)
        {
            MatchTest(nonMatchingPattern, domain, expectedResult: false);
        }

        private static void MatchTest(string pattern, string domain, bool expectedResult)
        {
            var domainPattern = new DomainPattern(pattern);
            var matchResult = domainPattern.Match(domain);

            Assert.Equal(matchResult, expectedResult);
        }
    }
}