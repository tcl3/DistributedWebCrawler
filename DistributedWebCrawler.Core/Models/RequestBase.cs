using System;

namespace DistributedWebCrawler.Core.Models
{
    public abstract class RequestBase
    {
        public RequestBase(Uri uri)
        {
            if (uri == null || !uri.IsAbsoluteUri)
            {
                throw new UriFormatException("Request URI must be absolute");
            }
            
            Uri = uri;
            Id = Guid.NewGuid();
            CreatedAt = SystemClock.DateTimeOffsetNow();
        }
        public Uri Uri { get; }
        public Guid Id { get; init; }
        public DateTimeOffset CreatedAt { get; init; }
    }
}