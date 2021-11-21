using System;
using System.Text.Json.Serialization;

namespace DistributedWebCrawler.Core.Model
{
    public class ParseRequest : RequestBase
    {
        [JsonConstructor]
        public ParseRequest() : base()
        {
            Uri = default!;
            IngestResult = default!;
        }

        public ParseRequest(Uri uri, IngestResult ingestResult) : base()
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
