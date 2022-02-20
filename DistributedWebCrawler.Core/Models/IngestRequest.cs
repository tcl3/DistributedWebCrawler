using System;

namespace DistributedWebCrawler.Core.Models
{
    public class IngestRequest : RequestBase
    {
        public IngestRequest(Uri uri) : base(uri)
        {
        }

        public int CurrentCrawlDepth { get; init; }
        public bool MaxDepthReached { get; init; }
    }
}
