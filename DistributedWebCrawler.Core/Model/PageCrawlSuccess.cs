using System;

namespace DistributedWebCrawler.Core.Model
{
    public class PageCrawlSuccess
    {
        public PageCrawlSuccess(IngestResult ingestResult)
        {
            Uri = ingestResult.Uri;
            RequestStartTime = ingestResult.RequestStartTime;
            TimeTaken = ingestResult.TimeTaken;
            ContentLength = ingestResult.ContentLength;
            ContentId = ingestResult.ContentId ?? throw new ArgumentNullException(nameof(ContentId));
            MediaType = ingestResult.MediaType ?? string.Empty;
        }

        public Uri Uri { get; init; }
        public DateTimeOffset RequestStartTime { get; init; }
        public TimeSpan TimeTaken { get; init; }

        public Guid ContentId { get; init; }
        public int ContentLength { get; init; }
        public string MediaType { get; init; }
    }
}
