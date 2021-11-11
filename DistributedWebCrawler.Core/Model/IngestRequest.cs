using System;

namespace DistributedWebCrawler.Core.Model
{
    public class IngestRequest
    {
        public IngestRequest(Uri host)
        {
            if (host == null || !host.IsAbsoluteUri)
            {
                throw new UriFormatException();
            }

            Uri = host;
        }
        public Uri Uri { get; }

        public int CurrentCrawlDepth { get; set; }
        public bool MaxDepthReached { get; set; }
    }
}
