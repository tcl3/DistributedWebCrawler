using System;
using System.Text.Json.Serialization;

namespace DistributedWebCrawler.Core.Model
{
    public class ParseRequest
    {
        [JsonConstructor]
        public ParseRequest()
        {
            Uri = default!;
            IngestResult = default!;
        }

        public ParseRequest(Uri uri, IngestResult ingestResult)
        {
            if (!uri.IsAbsoluteUri)
            {
                throw new UriFormatException();
            }

            Uri = uri;
            IngestResult = ingestResult;
        }

        public Uri Uri { get; set; }
        public IngestResult IngestResult { get; set; }
        public int CurrentCrawlDepth { get; set; }
    }
}
