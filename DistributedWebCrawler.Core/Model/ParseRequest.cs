using System;

namespace DistributedWebCrawler.Core.Model
{
    public class ParseRequest : RequestBase
    {
        public ParseRequest(Uri uri, Guid contentId, int currentCrawlDepth) : base()
        {
            if (!uri.IsAbsoluteUri)
            {
                throw new UriFormatException();
            }

            Uri = uri;
            ContentId = contentId;
            CurrentCrawlDepth = currentCrawlDepth;
            
        }

        public Uri Uri { get; init; }
        public Guid ContentId { get; init; }
        public int CurrentCrawlDepth { get; init; }
    }
}
