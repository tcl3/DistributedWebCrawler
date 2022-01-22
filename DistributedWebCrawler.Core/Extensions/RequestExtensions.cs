using DistributedWebCrawler.Core.Components;
using DistributedWebCrawler.Core.Interfaces;
using DistributedWebCrawler.Core.Model;

namespace DistributedWebCrawler.Core.Extensions
{
    public static class RequestExtensions
    {
        public static QueuedItemResult<TSuccess> Success<TSuccess>(this RequestBase request, TSuccess result)
        {
            return new QueuedItemResult<TSuccess>(request.Id, QueuedItemStatus.Success, result);
        }

        public static QueuedItemResult<TFailure> Failed<TFailure>(this RequestBase request, TFailure result)
            where TFailure : IErrorCode
        {
            return new QueuedItemResult<TFailure>(request.Id, QueuedItemStatus.Failed, result);
        }

        public static QueuedItemResult Waiting(this RequestBase request)
        {
            return new QueuedItemResult(request.Id, QueuedItemStatus.Waiting);
        }
    }
}
