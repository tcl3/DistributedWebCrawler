using System;
using System.Text.Json.Serialization;

namespace DistributedWebCrawler.Core.Models
{
    public class IngestRequest : RequestBase
    {
        public IngestRequest(Uri uri) : base()
        {
            if (uri == null || !uri.IsAbsoluteUri)
            {
                throw new UriFormatException();
            }

            Uri = uri;
        }

        public Uri Uri { get; init; }

        public int CurrentCrawlDepth { get; init; }
        public bool MaxDepthReached { get; init; }
    }
}
