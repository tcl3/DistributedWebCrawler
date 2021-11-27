using DistributedWebCrawler.Core.Extensions;
using DistributedWebCrawler.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NRobotsCore = Robots;

namespace DistributedWebCrawler.Core.Robots
{
    public class RobotsCacheReader : IRobotsCacheReader
    {
        private readonly IKeyValueStore _keyValueStore;
        private readonly RobotsCacheSettings _cacheSettings;

        public RobotsCacheReader(IKeyValueStore keyValueStore, RobotsCacheSettings cacheSettings)
        {
            _keyValueStore = keyValueStore.WithKeyPrefix(cacheSettings.KeyPrefix);
            _cacheSettings = cacheSettings;
        }

        public async Task<bool> GetRobotsTxtAsync(Uri uri, Action<IRobots>? ifExistsAction, CancellationToken cancellationToken)
        {
            var robotsString = await _keyValueStore.GetAsync(uri.Authority, cancellationToken).ConfigureAwait(false);

            if (robotsString == null)
            {
                return false;
            }

            if (ifExistsAction != null && robotsString != string.Empty)
            {
                var robots = new RobotsImpl(uri, robotsString, _cacheSettings.UserAgent);
                ifExistsAction(robots);
            }

            return true;
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
