using DistributedWebCrawler.Core.Configuration;
using DistributedWebCrawler.Core.Extensions;
using DistributedWebCrawler.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NRobotsCore = Robots;

namespace DistributedWebCrawler.Core.Robots
{
    // TODO: Handle redirects without storing duplicate entries
    public class RobotsCache : IRobotsCache
    {
        private readonly IKeyValueStore _keyValueStore;
        private readonly RobotsClient _robotsClient;

        private readonly TimeSpan? _expirationTimeSpan;

        private const string KeyPrefix = "RobotsTxt";

        public RobotsCache(IKeyValueStore keyValueStore, RobotsClient robotsClient, RobotsTxtSettings robotsTxtSettings)
        {
            _keyValueStore = keyValueStore.WithKeyPrefix(KeyPrefix);
            _robotsClient = robotsClient;
            _expirationTimeSpan = TimeSpan.FromSeconds(robotsTxtSettings.CacheIntervalSeconds);
        }      

        public async Task<bool> GetRobotsTxtAsync(Uri uri, Action<IRobots> ifExistsAction, CancellationToken cancellationToken)
        {
            var robotsString = await _keyValueStore.GetAsync(uri.Authority, cancellationToken).ConfigureAwait(false);

            if (robotsString == null)
            {
                return false;
            }

            if (ifExistsAction != null && robotsString != string.Empty)
            {                
                var robots = new RobotsImpl(uri, robotsString, _robotsClient.UserAgent);
                ifExistsAction(robots);
            }

            return true;
        }

        public async Task AddOrUpdateRobotsForHostAsync(Uri host, CancellationToken cancellationToken)
        {
            // if RobotsClient fails to get content, add an empty string entry to the cache, so we only rerequest after the caching interval has expired
            var success = await _robotsClient.TryGetRobotsAsync(host, async contentFromClient =>
            {
                await _keyValueStore.PutAsync(host.Authority, contentFromClient, cancellationToken, _expirationTimeSpan).ConfigureAwait(false);
            }, cancellationToken).ConfigureAwait(false);

            if (!success)
            {
                await _keyValueStore.PutAsync(host.Authority, string.Empty, cancellationToken, _expirationTimeSpan).ConfigureAwait(false);
            }            
        }

        private class RobotsImpl : IRobots
        {
            private int? _crawlDelay;
            public int CrawlDelay
            {
                get
                {
                    if (_crawlDelay == null)
                    {
                        _crawlDelay = _robots.GetCrawlDelay(_userAgent);
                    }

                    return _crawlDelay.Value;
                } 
            }

            public IEnumerable<string> SitemapUrls => _robots.GetSitemapUrls();

            private readonly NRobotsCore.Robots _robots;
            private readonly string _userAgent;

            private const string AllAgents = "*";

            public RobotsImpl(Uri host, string content, string? userAgent)
            {
                _robots = new NRobotsCore.Robots();
                _robots.LoadContent(content, host);
                _userAgent = userAgent ?? AllAgents;
            }

            public bool Allowed(Uri uri)
            {
                return _robots.Allowed(uri, _userAgent);
            }
        }
    }
}