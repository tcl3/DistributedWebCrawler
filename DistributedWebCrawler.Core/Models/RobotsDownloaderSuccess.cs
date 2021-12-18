using System;

namespace DistributedWebCrawler.Core.Model
{
    public class RobotsDownloaderSuccess
    {
        public Uri Uri { get; init; }
        public RobotsDownloaderSuccess(Uri uri)
        {
            Uri = uri;
        }
    }
}
