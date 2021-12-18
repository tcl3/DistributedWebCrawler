using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace DistributedWebCrawler.Core.Model
{
    public class IngestSuccess
    {
        public IngestSuccess(Uri uri, DateTimeOffset requestStartTime) : base()
        {
            Uri = uri;
            RequestStartTime = requestStartTime;
            TimeTaken = DateTimeOffset.Now - requestStartTime;
        }
        
        public HttpStatusCode? HttpStatusCode { get; init; }

        public Uri Uri { get; init; }
        public DateTimeOffset RequestStartTime { get; init; }
        public TimeSpan TimeTaken { get; init; }
        public Guid? ContentId { get; init; } 
        public int ContentLength { get; init; }
        public string MediaType { get; init; } = string.Empty;

        public IEnumerable<RedirectResult> Redirects { get; init; } = Enumerable.Empty<RedirectResult>();

        public static IngestSuccess Success(Uri uri, DateTimeOffset requestStartTime, Guid contentId, int contentLength, string mediaType, IEnumerable<RedirectResult>? redirects = null)
        {
            return new IngestSuccess(uri, requestStartTime)
            { 
                ContentId = contentId,
                ContentLength = contentLength,
                MediaType = mediaType,
                Redirects = redirects ?? Enumerable.Empty<RedirectResult>()
            };
        }
    }
}
