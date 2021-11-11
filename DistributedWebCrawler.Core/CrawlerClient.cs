using System;
using System.Net.Http;
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

        public Task<HttpResponseMessage> GetAsync(Uri uri)
        {
            return _client.GetAsync(uri);
        }
    }
}
