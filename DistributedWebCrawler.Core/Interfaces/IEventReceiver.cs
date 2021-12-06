using DistributedWebCrawler.Core.Components;

namespace DistributedWebCrawler.Core.Interfaces
{
    public interface IEventReceiver
    {
        event ItemCompletedEventHandler OnCompletedAsync;
        event ItemCompletedEventHandler OnFailedAsync;
        event AsyncEventHandler<ComponentStatus> OnComponentUpdateAsync;
    }

    public interface IEventReceiver<TSuccess, TFailure> : IEventReceiver
    {
        new event ItemCompletedEventHandler<TSuccess> OnCompletedAsync;
        new event ItemCompletedEventHandler<TFailure> OnFailedAsync;
    }
}
