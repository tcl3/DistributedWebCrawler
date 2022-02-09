using System;
using System.Text.Json.Serialization;

namespace DistributedWebCrawler.Core.Models
{
    public class IngestRequest : RequestBase
    {
        [JsonConstructor]
        public IngestRequest() : base()
        {
            Uri = default!;
        }

        public IngestRequest(Uri host) : base()
        {
            if (host == null || !host.IsAbsoluteUri)
            {
                throw new UriFormatException();
            }

            Uri = host;
        }

        public Uri Uri { get; set; }

        public int CurrentCrawlDepth { get; set; }
        public bool MaxDepthReached { get; set; }
    }
}
