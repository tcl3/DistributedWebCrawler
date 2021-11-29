using DistributedWebCrawler.Core.Interfaces;
using DistributedWebCrawler.Core.Model;

namespace DistributedWebCrawler.Core.Queue
{
    public class InMemoryEventReceiver<TRequest, TResult> : IEventReceiver<TRequest, TResult> 
        where TRequest : RequestBase
    {
        private readonly InMemoryEventStore<TResult> _eventStore;

        public InMemoryEventReceiver(InMemoryEventStore<TResult> eventStore)
        {
            _eventStore = eventStore;
        }
        
        public event ItemCompletedEventHandler<TResult> OnCompletedAsync
        {
            add
            {
                _eventStore.OnCompletedAsyncHandler += value;
            }
            remove
            {
                _eventStore.OnCompletedAsyncHandler -= value;
            }
        }
    }
}
