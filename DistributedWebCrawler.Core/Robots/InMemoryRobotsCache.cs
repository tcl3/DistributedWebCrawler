using DistributedWebCrawler.Core.Configuration;
using DistributedWebCrawler.Core.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using NRobotsCore = Robots;

namespace DistributedWebCrawler.Core.Exceptions
{
    public class InMemoryRobotsCache : IRobotsCache
    {
        private readonly TimeSpan _cacheInterval;
        private readonly ConcurrentDictionary<string, CacheEntry> _robotsCache;
        private readonly RobotsClient _robotsClient;

        public InMemoryRobotsCache(RobotsTxtSettings robotsSettings,
            RobotsClient robotsClient)
        {
            _robotsCache = new();
            _cacheInterval = TimeSpan.FromSeconds(robotsSettings.CacheIntervalSeconds);
            _robotsClient = robotsClient;
        }
        public async Task GetRobotsForHostAsync(Uri uri, Action<IRobots> ifExistsAction)
        {
            if (!uri.IsAbsoluteUri)
            {
                throw new UriFormatException($"Expected absolute URI when fetching robots.txt. Uri: '{uri}'");
            }

            var authority = uri.GetLeftPart(UriPartial.Authority);
            var cacheEntry = _robotsCache.AddOrUpdate(authority, AddCacheEntry, UpdateCacheEntry);

            var robots = await cacheEntry.Robots.ConfigureAwait(false);
            if (robots != null)
            {
                ifExistsAction?.Invoke(robots);
            }
        }

        private async Task<IRobots?> AddRobotsForHostAsync(string authority)
        {
            var authorityUri = new Uri(authority, UriKind.Absolute);

            try
            {
                var robotsResponse = await _robotsClient.GetAsync(authorityUri).ConfigureAwait(false);


                if (!robotsResponse.IsSuccessStatusCode)
                {
                    return null;
                }

                var robotsTxtString = await robotsResponse.Content.ReadAsStringAsync().ConfigureAwait(false);


                if (string.IsNullOrEmpty(robotsTxtString))
                {
                    return null;
                }

                var userAgentString = _robotsClient.UserAgent;

                IRobots robots = new RobotsImpl(authority, robotsTxtString, userAgentString);
                return robots;

            } catch (HttpRequestException)
            {
                // Returning null here means that we will not retry getting robots.txt every time we make a request for its domain
                return null;
            }            
        }

        private CacheEntry AddCacheEntry(string authority)
        {
            var robots = AddRobotsForHostAsync(authority);
            return new CacheEntry(robots, DateTimeOffset.Now);
        }

        private CacheEntry UpdateCacheEntry(string authority, CacheEntry oldValue)
        {
            var shouldUpdate = DateTimeOffset.Now - oldValue.CreatedAt >= _cacheInterval;
            return shouldUpdate ? AddCacheEntry(authority) : oldValue;
        }

        private class CacheEntry
        {
            public Task<IRobots?> Robots { get; }
            public DateTimeOffset CreatedAt { get; }

            public CacheEntry(Task<IRobots?> robots, DateTimeOffset createdAt)
            {
                Robots = robots;
                CreatedAt = createdAt;
            }
        }

        private class RobotsImpl : IRobots
        {
            public Uri BaseUri => _robots.BaseUri;

            public int CrawlDelay { get; }

            public IEnumerable<string> SitemapUrls => _robots.GetSitemapUrls();

            private readonly NRobotsCore.Robots _robots;

            public RobotsImpl(string host, string content, string? userAgent)
            {
                _robots = new NRobotsCore.Robots();
                _robots.LoadContent(content, host);

                CrawlDelay = GetCrawlDelay(_robots, userAgent);
            }

            private static int GetCrawlDelay(NRobotsCore.IRobots robots, string? userAgent)
            {
                var delayForUserAgent = string.IsNullOrWhiteSpace(userAgent)
                    ? 0
                    : robots.GetCrawlDelay(userAgent);

                return delayForUserAgent > 0 ? delayForUserAgent : robots.GetCrawlDelay();
            }

            public bool Allowed(string path)
            {
                return _robots.Allowed(path);
            }
        }
    }
}
