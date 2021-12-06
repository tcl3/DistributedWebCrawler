using DistributedWebCrawler.Core.Model;
using System;

namespace DistributedWebCrawler.Core.Components
{
    public class QueuedItemResult<TResult> : QueuedItemResult
    {
        public TResult Result { get; }

        private QueuedItemResult(Guid requestId, QueuedItemStatus status, TResult result) : base(requestId, status)
        {
            Result = result;
        }

        public static QueuedItemResult<TResult> Success(RequestBase request, TResult result)
        {
            return new QueuedItemResult<TResult>(request.Id, QueuedItemStatus.Success, result);
        }

        public static QueuedItemResult<TResult> Failed(RequestBase request, TResult result)
        {
            return new QueuedItemResult<TResult>(request.Id, QueuedItemStatus.Failed, result);
        }
    }

    public class QueuedItemResult
    {
        public Guid RequestId { get; }
        public QueuedItemStatus Status { get; }

        protected QueuedItemResult(Guid requestId, QueuedItemStatus status)
        {
            RequestId = requestId;
            Status = status;
        }

        public static QueuedItemResult<TSuccess> Success<TSuccess>(RequestBase request, TSuccess result)
        {
            return QueuedItemResult<TSuccess>.Success(request, result);
        }

        public static QueuedItemResult<TFailure> Failed<TFailure>(RequestBase request, TFailure result)
        {
            return QueuedItemResult<TFailure>.Failed(request, result);
        }

        public static QueuedItemResult Waiting(RequestBase request)
        {
            return new QueuedItemResult(request.Id, QueuedItemStatus.Waiting);
        }
    }
}
