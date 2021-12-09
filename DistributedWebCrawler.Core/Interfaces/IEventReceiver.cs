using DistributedWebCrawler.Core.Components;

namespace DistributedWebCrawler.Core.Interfaces
{
    public interface IEventReceiver
    {
        event ItemCompletedEventHandler OnCompletedAsync;
        event ItemCompletedEventHandler OnFailedAsync;
        event ComponentEventHandler<ComponentStatus> OnComponentUpdateAsync;
    }

    public interface IEventReceiver<TSuccess, TFailure> : IEventReceiver
        where TSuccess : notnull
        where TFailure : notnull
    {
        new event ItemCompletedEventHandler<TSuccess> OnCompletedAsync;
        new event ItemCompletedEventHandler<TFailure> OnFailedAsync;
    }
}
