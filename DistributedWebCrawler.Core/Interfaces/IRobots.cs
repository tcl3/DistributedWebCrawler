using System;
using System.Collections.Generic;

namespace DistributedWebCrawler.Core.Interfaces
{
    public interface IRobots
    {
        int CrawlDelay { get; }
        IEnumerable<string> SitemapUrls { get; }
        bool Allowed(Uri uri);
    }
}
