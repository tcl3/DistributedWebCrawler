using DistributedWebCrawler.Core.Tests.Attributes;
using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace DistributedWebCrawler.Core.Tests
{
    public class FallbackEncodingHandlerTests
    {
        [Theory]
        [HttpClientWithFallbackEncodingAutoData(charset: null)]
        public async Task ResponseWithNullCharsetShouldNotBeModified(Uri uri, HttpClient client)
        {
            var response = await client.GetAsync(uri);
            Assert.Null(response.Content.Headers.ContentType!.CharSet);
        }

        [Theory]
        [HttpClientWithFallbackEncodingAutoData(charset: "UTF-16")]
        public async Task ResponseWithValidCharsetShouldNotBeModified(Uri uri, HttpClient client)
        {
            var response = await client.GetAsync(uri);
            Assert.Equal("UTF-16", response.Content.Headers.ContentType!.CharSet);
        }

        [Theory]
        [HttpClientWithFallbackEncodingAutoData(charset: "NOT-A-REAL-CHARSET")]
        public async Task ResponseWithInvalidCharsetShouldHaveCharsetReplacedByFallback(Uri uri, Encoding fallbackEncoding, HttpClient client)
        {
            var response = await client.GetAsync(uri);
            Assert.Equal(fallbackEncoding.WebName, response.Content.Headers.ContentType!.CharSet);
        }

        [Theory]
        [HttpClientWithFallbackEncodingAutoData(charset: "NOT-A-REAL-CHARSET")]
        public async Task ExceptionShouldBeThrownWhenTimeoutOccursWhileReadingResponse(Uri uri, HttpClient client)
        {
            var cts = new CancellationTokenSource();
            cts.Cancel();
            await Assert.ThrowsAsync<TaskCanceledException>(() => client.GetAsync(uri, cts.Token));
        }
    }
}
