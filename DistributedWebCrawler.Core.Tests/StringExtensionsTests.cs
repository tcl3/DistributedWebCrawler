using DistributedWebCrawler.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace DistributedWebCrawler.Core.Tests
{
    public class StringExtensionsTests
    {
        [Fact]
        public void Test()
        {
            Assert.Throws<ArgumentNullException>(() => StringExtensions.GetCommonPrefix(null!, "second"));
            Assert.Throws<ArgumentNullException>(() => StringExtensions.GetCommonPrefix("first", null!));        
        }

        [Theory]
        [InlineData("", "", "")]
        [InlineData("first", "", "")]
        [InlineData("", "second", "")]
        [InlineData("first", "second", "")]
        [InlineData("equal", "equal", "equal")]
        [InlineData("prefix_first", "prefix_second", "prefix_")]
        public void Test2(string first, string second, string expectedResult)
        {
            Assert.Equal(expectedResult, first.GetCommonPrefix(second));
            Assert.Equal(expectedResult, second.GetCommonPrefix(first));
        }
    }
}
