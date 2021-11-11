using System;

namespace DistributedWebCrawler.Core.Model
{
    public class ParseRequest
    {
        public ParseRequest(Uri uri, IngestResult ingestResult)
        {
            if (!uri.IsAbsoluteUri)
            {
                throw new UriFormatException();
            }

            Uri = uri;
            IngestResult = ingestResult;
        }

        public Uri Uri { get; }
        public IngestResult IngestResult { get; }
        public int CurrentCrawlDepth { get; set; }
    }
}
