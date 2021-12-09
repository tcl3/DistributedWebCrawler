using System.Collections.Generic;

namespace DistributedWebCrawler.Core.Interfaces
{
    public interface IEventReceiverFactory
    {
        IEnumerable<IEventReceiver> GetAll();
        IEventReceiver<TSuccess, TFailure> Get<TSuccess, TFailure>()
            where TSuccess : notnull
            where TFailure : notnull;
    }
}
