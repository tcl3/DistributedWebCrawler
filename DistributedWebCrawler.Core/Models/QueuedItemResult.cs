using System;

namespace DistributedWebCrawler.Core.Models
{
    public record QueuedItemResult<TResult>(Guid RequestId, QueuedItemStatus Status, TResult Result) : QueuedItemResult(RequestId, Status);
    public record QueuedItemResult(Guid RequestId, QueuedItemStatus Status);
}
