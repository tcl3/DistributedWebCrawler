using System;

namespace DistributedWebCrawler.Core.Model
{
    public abstract class RequestBase
    {
        public RequestBase()
        {
            Id = Guid.NewGuid();
            CreatedAt = DateTimeOffset.Now;
        }

        public Guid Id { get; init; }
        public DateTimeOffset CreatedAt { get; init; }
    }
}