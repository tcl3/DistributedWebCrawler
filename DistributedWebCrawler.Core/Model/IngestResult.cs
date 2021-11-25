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

    public class IngestResult : RequestBase
    {
        public IngestResult(Uri uri) : base()
        {
            Uri = uri;
        }

        public IngestFailureReason? IngestFailureReason { get; init; }
        public HttpStatusCode? HttpStatusCode { get; init; }

        public Uri Uri { get; init; }
        public Guid? ContentId { get; init; } 
        public string? MediaType { get; init; } = string.Empty;

        public IEnumerable<RedirectResult> Redirects { get; init; } = Enumerable.Empty<RedirectResult>();

        public static IngestResult Failure(Uri uri, IngestFailureReason failureReason, HttpStatusCode? httpStatusCode = null, string? mediaType = null, IEnumerable<RedirectResult>? redirects = null)
        {
            return new IngestResult(uri)
            { 
                IngestFailureReason = failureReason,
                HttpStatusCode = httpStatusCode,
                MediaType = mediaType,
            };
        }

        public static IngestResult Success(Uri uri, Guid contentId, string mediaType, IEnumerable<RedirectResult>? redirects = null)
        {
            return new IngestResult(uri)
            { 
                ContentId = contentId, 
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
