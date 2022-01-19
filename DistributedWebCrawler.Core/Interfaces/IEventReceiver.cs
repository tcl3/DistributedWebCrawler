using DistributedWebCrawler.Core.Components;

namespace DistributedWebCrawler.Core.Interfaces
{
    public interface IEventReceiver
    {
        event ItemCompletedEventHandler OnCompletedAsync;
        event ItemFailedEventHandler OnFailedAsync;
        event ComponentEventHandler<ComponentStatus> OnComponentUpdateAsync;
    }

    public interface IEventReceiver<TSuccess, TFailure>
        where TSuccess : notnull
        where TFailure : notnull, IErrorCode
    {
        event ItemCompletedEventHandler<TSuccess> OnCompletedAsync;
        event ItemFailedEventHandler<TFailure> OnFailedAsync;
    }
}
