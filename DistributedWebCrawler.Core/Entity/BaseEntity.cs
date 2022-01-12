using System;

namespace DistributedWebCrawler.Core.Entity
{
    public abstract class BaseEntity<TKey> where TKey : struct
    {
        protected BaseEntity(DateTimeOffset? createdAt = null, DateTimeOffset? lastModified = null)
        {
            CreatedAt = createdAt ?? SystemClock.DateTimeOffsetNow();
            LastModified = lastModified ?? SystemClock.DateTimeOffsetNow();
        }

        public TKey Key { get; set; }
        public DateTimeOffset CreatedAt { get; }
        public DateTimeOffset LastModified { get; }
    }
}
