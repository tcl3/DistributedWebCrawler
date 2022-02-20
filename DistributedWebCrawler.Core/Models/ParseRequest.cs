using System;

namespace DistributedWebCrawler.Core.Models
{
    public class ParseRequest : RequestBase
    {
        public ParseRequest(Uri uri, Guid contentId, int currentCrawlDepth) : base(uri)
        {
            ContentId = contentId;
            CurrentCrawlDepth = currentCrawlDepth;    
        }
        
        public Guid ContentId { get; init; }
        public int CurrentCrawlDepth { get; init; }
    }
}
