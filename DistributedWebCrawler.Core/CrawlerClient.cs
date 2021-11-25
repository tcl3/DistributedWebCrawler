using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace DistributedWebCrawler.Core
{
    public class CrawlerClient
    {
        private readonly HttpClient _client;

        public CrawlerClient(HttpClient client)
        {
            _client = client;
        }

        public Task<HttpResponseMessage> GetAsync(Uri uri, CancellationToken cancellationToken)
        {
            return _client.GetAsync(uri, cancellationToken);
        }
    }
}
