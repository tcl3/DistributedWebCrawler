using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace DistributedWebCrawler.Core
{
    public class RobotsClient
    {
        private readonly HttpClient _client;
        private readonly ILogger<RobotsClient> _logger;

        public RobotsClient(HttpClient client, ILogger<RobotsClient> logger)
        {
            _client = client;
            _logger = logger;
        }

        public async Task<bool> TryGetRobotsAsync(Uri host, Func<string, Task> robotsTxtExistsAction, CancellationToken cancellationToken)
        {
            var uri = new Uri(host, "/robots.txt");
            try
            {
                var response = await _client.GetAsync(uri, cancellationToken).ConfigureAwait(false);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                    if (!string.IsNullOrEmpty(content))
                    {
                        await robotsTxtExistsAction(content).ConfigureAwait(false);
                        return true;
                    } 
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, $"Error when getting robots.txt for host: {host}");
            }

            return false;
        }

        public string? UserAgent => _client.DefaultRequestHeaders.UserAgent?.ToString();
    }
}