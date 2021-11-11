using System.Net;
using System.Net.Http;

namespace DistributedWebCrawler.Core
{
    public class CrawlerHttpClientHandler : HttpClientHandler
    {
        public CrawlerHttpClientHandler() : base()
        {
            AllowAutoRedirect = false;
            AutomaticDecompression = DecompressionMethods.All;
        }
    }
}
