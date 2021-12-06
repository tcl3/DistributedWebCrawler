using DistributedWebCrawler.Core.Model;

namespace DistributedWebCrawler.Core.Interfaces
{
    public interface IEventReceiver
    {
        event ItemCompletedEventHandler OnCompletedAsync;
        event ItemCompletedEventHandler OnFailedAsync;
    }

    public interface IEventReceiver<TSuccess, TFailure> : IEventReceiver
    {
        new event ItemCompletedEventHandler<TSuccess> OnCompletedAsync;
        new event ItemCompletedEventHandler<TFailure> OnFailedAsync;
    }
}
