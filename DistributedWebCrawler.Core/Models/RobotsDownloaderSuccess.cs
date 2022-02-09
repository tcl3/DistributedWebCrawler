using System;

namespace DistributedWebCrawler.Core.Models
{
    public class RobotsDownloaderSuccess
    {
        public Uri Uri { get; init; }
        public int ContentLength { get; init; }
        public RobotsDownloaderSuccess(Uri uri)
        {
            Uri = uri;
        }
    }
}
