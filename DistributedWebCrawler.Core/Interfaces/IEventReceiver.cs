using DistributedWebCrawler.Core.Model;

namespace DistributedWebCrawler.Core.Interfaces
{
    public interface IEventReceiver<TRequest, TResult>
        where TRequest : RequestBase
    {
        event ItemCompletedEventHandler<TResult> OnCompletedAsync;
    }
}
