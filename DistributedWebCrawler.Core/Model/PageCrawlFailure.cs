using System;
using System.Collections.Generic;
using System.Net;

namespace DistributedWebCrawler.Core.Model
{
    public class PageCrawlFailure
    {
        public PageCrawlFailure(IngestResult ingestResult)
        {
            Uri = ingestResult.Uri;
            FailureReason = ingestResult.FailureReason ?? throw new ArgumentNullException(nameof(FailureReason));
            StatusCode = ingestResult.HttpStatusCode;
            RequestStartTime = ingestResult.RequestStartTime;
            TimeTaken = ingestResult.TimeTaken;
        }

        public Uri Uri { get; init; }
        public IngestFailureReason FailureReason { get; init; }

        public HttpStatusCode? StatusCode { get; init; }

        public DateTimeOffset RequestStartTime { get; init; }
        public TimeSpan TimeTaken { get; init; }
    }
}
