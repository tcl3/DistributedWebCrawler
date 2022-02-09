using System;

namespace DistributedWebCrawler.Core.Models
{
    public class ParseSuccess
    {
        public Uri Uri { get; init; }
        public int NumberOfLinks { get; init; }

        public ParseSuccess(Uri uri)
        {
            Uri = uri;
        }
    }
}
