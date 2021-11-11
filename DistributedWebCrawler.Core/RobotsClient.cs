using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace DistributedWebCrawler.Core
{
    public class RobotsClient
    {
        private readonly HttpClient _client;

        public RobotsClient(HttpClient client)
        {
            _client = client;
        }

        public Task<HttpResponseMessage> GetAsync(Uri host)
        {
            var uri = new Uri(host, "/robots.txt");
            return _client.GetAsync(uri);
        }

        public string? UserAgent => _client.DefaultRequestHeaders.UserAgent?.ToString();
    }
}