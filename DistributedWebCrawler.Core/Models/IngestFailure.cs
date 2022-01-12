using DistributedWebCrawler.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace DistributedWebCrawler.Core.Model
{
    public class IngestFailure : IErrorCode<IngestFailureReason>
    {
        public Uri Uri { get; init; }
        public DateTimeOffset RequestStartTime { get; init; }
        public TimeSpan TimeTaken { get; init; }
        public HttpStatusCode? HttpStatusCode { get; init; }
        public string? MediaType { get; init; } = string.Empty;
        public IEnumerable<RedirectResult> Redirects { get; init; } = Enumerable.Empty<RedirectResult>();

        public IngestFailureReason Error { get; init; }

        Enum IErrorCode.Error => Error;

        public IngestFailure(Uri uri, DateTimeOffset requestStartTime) : base()
        {
            Uri = uri;
            RequestStartTime = requestStartTime;
            TimeTaken = SystemClock.DateTimeOffsetNow() - requestStartTime;
        }

        public static IngestFailure Create(Uri uri, DateTimeOffset requestStartTime, IngestFailureReason errorCode, HttpStatusCode? httpStatusCode = null, string? mediaType = null, IEnumerable<RedirectResult>? redirects = null)
        {
            return new IngestFailure(uri, requestStartTime)
            {
                Error = errorCode,
                HttpStatusCode = httpStatusCode,
                MediaType = mediaType,
                Redirects = redirects ?? Enumerable.Empty<RedirectResult>()
            };
        }
    }
}
