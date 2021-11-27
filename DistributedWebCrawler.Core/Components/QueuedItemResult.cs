using DistributedWebCrawler.Core.Model;
using System;

namespace DistributedWebCrawler.Core.Components
{
    public class QueuedItemResult<TResult>
    {
        public Guid RequestId { get; }
        public QueuedItemStatus Status { get; }
        public TResult? Result { get; }

        private QueuedItemResult(Guid requestId, QueuedItemStatus status, TResult? result = default)
        {
            RequestId = requestId;
            Status = status;
            Result = result;
        }

        public static QueuedItemResult<TResult> Completed<TRequest>(TRequest request, TResult result)
            where TRequest : RequestBase
        {
            return new QueuedItemResult<TResult>(request.Id, QueuedItemStatus.Completed, result);
        }

        public static QueuedItemResult<TResult> Waiting<TRequest>(TRequest request)
            where TRequest : RequestBase
        {
            return new QueuedItemResult<TResult>(request.Id, QueuedItemStatus.Waiting);
        }
    }

    internal static class QueuedItemResult
    {
        public static QueuedItemResult<TResult> Completed<TResult>(RequestBase request, TResult result)
        {
            return QueuedItemResult<TResult>.Completed(request, result);
        }

        public static QueuedItemResult<TResult> Waiting<TResult>(RequestBase request)
        {
            return QueuedItemResult<TResult>.Waiting(request);
        }
    }
}
