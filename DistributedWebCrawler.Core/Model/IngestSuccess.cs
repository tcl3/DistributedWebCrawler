using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace DistributedWebCrawler.Core.Model
{

    public enum IngestFailureReason
    {
        None = 0,
        UnknownError,
        MaxDepthReached,
        Http4xxError,
        NetworkConnectivityError,
        UriFormatError,
        RequestTimeout,
        ContentTooLarge,
        MediaTypeNotPermitted,
        MaxRedirectsReached

    }

    public class IngestFailure
    {
        public Uri Uri { get; init; }
        public DateTimeOffset RequestStartTime { get; init; }
        public TimeSpan TimeTaken { get; init; }

        public IngestFailureReason FailureReason { get; init; }
        public HttpStatusCode? HttpStatusCode { get; init; }
        public string? MediaType { get; init; } = string.Empty;
        public IEnumerable<RedirectResult> Redirects { get; init; } = Enumerable.Empty<RedirectResult>();

        public IngestFailure(Uri uri, DateTimeOffset requestStartTime) : base()
        {
            Uri = uri;
            RequestStartTime = requestStartTime;
            TimeTaken = DateTimeOffset.Now - requestStartTime;
        }

        public static IngestFailure Create(Uri uri, DateTimeOffset requestStartTime, IngestFailureReason failureReason, HttpStatusCode? httpStatusCode = null, string? mediaType = null, IEnumerable<RedirectResult>? redirects = null)
        {
            return new IngestFailure(uri, requestStartTime)
            {
                FailureReason = failureReason,
                HttpStatusCode = httpStatusCode,
                MediaType = mediaType,
                Redirects = redirects ?? Enumerable.Empty<RedirectResult>()
            };
        }
    }

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

    public class RedirectResult
    {
        public RedirectResult(Uri destinationUri, HttpStatusCode redirectStatusCode)
        {
            DestinationUri = destinationUri;
            RedirectStatusCode = redirectStatusCode;
        }

        public Uri DestinationUri { get; init; }
        public HttpStatusCode RedirectStatusCode { get; init; }
    }
}
