using System;
using System.Collections.Generic;

namespace DistributedWebCrawler.Core.Interfaces
{
    public interface IRobots
    {
        Uri BaseUri { get; }
        int CrawlDelay { get; }
        IEnumerable<string> SitemapUrls { get; }
        bool Allowed(Uri uri);
    }
}
