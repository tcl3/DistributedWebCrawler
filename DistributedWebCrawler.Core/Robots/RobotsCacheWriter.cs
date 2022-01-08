using DistributedWebCrawler.Core.Configuration;
using DistributedWebCrawler.Core.Extensions;
using DistributedWebCrawler.Core.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace DistributedWebCrawler.Core.Robots
{
    // TODO: Handle redirects without storing duplicate entries
    public class RobotsCacheWriter : IRobotsCacheWriter
    {
        private readonly IKeyValueStore _keyValueStore;
        private readonly RobotsClient _robotsClient;
        public RobotsCacheWriter(IKeyValueStore keyValueStore, RobotsClient robotsClient, RobotsCacheSettings cacheSettings)
        {
            _keyValueStore = keyValueStore.WithKeyPrefix(cacheSettings.KeyPrefix);
            _robotsClient = robotsClient;
        }

        public async Task<string> AddOrUpdateRobotsForHostAsync(Uri host, TimeSpan expirationTimeSpan, CancellationToken cancellationToken)
        {
            // if RobotsClient fails to get content, add an empty string entry to the cache, so we only rerequest after the caching interval has expired
            var result = string.Empty;
            var success = await _robotsClient.TryGetRobotsAsync(host, async contentFromClient =>
            {
                await _keyValueStore.PutAsync(host.Authority, contentFromClient, cancellationToken, expirationTimeSpan).ConfigureAwait(false);
                result = contentFromClient;
            }, cancellationToken).ConfigureAwait(false);

            if (!success)
            {
                await _keyValueStore.PutAsync(host.Authority, string.Empty, cancellationToken, expirationTimeSpan).ConfigureAwait(false);
            }

            return result;
        }
    }
}