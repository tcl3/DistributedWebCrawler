using DistributedWebCrawler.Core.Components;

namespace DistributedWebCrawler.Core.Interfaces
{
    public interface IEventReceiver
    {
        event ItemCompletedEventHandler OnCompletedAsync;
        event ItemFailedEventHandler OnFailedAsync;
        event ComponentEventHandler<ComponentStatus> OnComponentUpdateAsync;
    }

    public interface IEventReceiver<TSuccess, TFailure> : IEventReceiver
        where TSuccess : notnull
        where TFailure : notnull, IErrorCode
    {
        new event ItemCompletedEventHandler<TSuccess> OnCompletedAsync;
        new event ItemFailedEventHandler<TFailure> OnFailedAsync;
    }
}
