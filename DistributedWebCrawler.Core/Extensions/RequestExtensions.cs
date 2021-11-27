using DistributedWebCrawler.Core.Components;
using DistributedWebCrawler.Core.Model;

namespace DistributedWebCrawler.Core.Extensions
{
    public static class RequestExtensions
    {
        public static QueuedItemResult<TResult> Completed<TResult>(this RequestBase requst, TResult result)
        {
            return QueuedItemResult.Completed(requst, result);
        }

        public static QueuedItemResult<TResult> Waiting<TResult>(this RequestBase request)
        {
            return QueuedItemResult.Waiting<TResult>(request);
        }

        public static QueuedItemResult<bool> Waiting(this RequestBase request)
        {
            return QueuedItemResult.Waiting<bool>(request);
        }
    }
}