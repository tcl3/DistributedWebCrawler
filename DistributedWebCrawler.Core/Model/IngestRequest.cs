using System;
using System.Text.Json.Serialization;

namespace DistributedWebCrawler.Core.Model
{
    public class IngestRequest
    {
        [JsonConstructor]
        public IngestRequest()
        {
            Uri = default!;
        }

        public IngestRequest(Uri host)
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
