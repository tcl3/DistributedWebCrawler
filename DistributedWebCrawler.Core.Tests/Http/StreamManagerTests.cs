using Xunit;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net;
using DistributedWebCrawler.Core.Tests.Attributes;
using System.Linq;
using DistributedWebCrawler.Core.Tests.Collections;

namespace DistributedWebCrawler.Core.Tests.Http
{
    // FIXME: Ideally these tests would be moved into their own project
    // They are closer to integration tests than unit tests, as they rely
    // on a HTTP server being set up.
    [Collection(nameof(StreamManagerCollection))]
    public class StreamManagerTests
    {
        [Theory]
        [StreamManagerAutoData]
        [StreamManagerAutoData(useCustomDns: true)]
        public async Task StreamManagerTotalBytesSentAndReceivedShouldBeUpdatedWhenConnectCallbackIsUsed(
            HttpListener listener, StreamManager sut)
        {
            try
            {
                var handler = new SocketsHttpHandler
                {
                    ConnectCallback = sut.ConnectCallback
                };

                using (var client = new HttpClient(handler, true))
                {
                    var address = listener.Prefixes.First();
                    var response = await client.GetAsync(address);
                    Assert.Equal(1, sut.ActiveSockets);
                }

                Assert.True(sut.TotalBytesSent > 0);
                Assert.True(sut.TotalBytesReceived > 0);

                Assert.Equal(0, sut.ActiveSockets);
            }
            finally
            {
                listener.Stop();
            }
        }

        [Theory]
        [StreamManagerAutoData(customResolverThrowsException: true)]
        public async Task HttpRequestExceptionShouldBePropagatedWhenDnsResolverThrowsException(
            HttpListener listener, StreamManager sut)
        {
            try
            {
                var handler = new SocketsHttpHandler
                {
                    ConnectCallback = sut.ConnectCallback
                };

                var address = listener.Prefixes.First();

                using (var client = new HttpClient(handler, true))
                {
                    await Assert.ThrowsAsync<HttpRequestException>(() => client.GetAsync(address));
                    Assert.Equal(0, sut.ActiveSockets);
                }

                Assert.Equal(0, sut.TotalBytesSent);
                Assert.Equal(0, sut.TotalBytesReceived);
            }
            finally
            {
                listener.Stop();
            }
        }
    }
}
