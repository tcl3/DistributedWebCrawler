using DistributedWebCrawler.Core.Components;
using DistributedWebCrawler.Core.Interfaces;

namespace DistributedWebCrawler.Core.Queue
{
    public class InMemoryEventReceiver<TSuccess, TFailure> : 
        IEventReceiver<TSuccess, TFailure>, IEventReceiver
        where TSuccess : notnull
        where TFailure : notnull, IErrorCode
    {
        private readonly InMemoryEventStore<TSuccess, TFailure> _eventStore;
        
        public InMemoryEventReceiver(InMemoryEventStore<TSuccess, TFailure> eventStore)
        {
            _eventStore = eventStore;
        }

        public event ItemCompletedEventHandler<TSuccess> OnCompletedAsync
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

        public event ItemFailedEventHandler<TFailure> OnFailedAsync
        {
            add
            {
                _eventStore.OnFailedAsyncHandler += value;
            }
            remove
            {
                _eventStore.OnFailedAsyncHandler -= value;
            }
        }

        event ItemCompletedEventHandler IEventReceiver.OnCompletedAsync
        {
            add
            {
                _eventStore.OnCompletedAsync += value;
            }
            remove
            {
                _eventStore.OnCompletedAsync -= value;
            }
        }

        event ItemFailedEventHandler IEventReceiver.OnFailedAsync
        {
            add
            {
                _eventStore.OnFailedAsync += value;
            }
            remove
            {
                _eventStore.OnFailedAsync -= value;
            }
        }

        public event ComponentEventHandler<ComponentStatus> OnComponentUpdateAsync
        {
            add
            {
                _eventStore.OnComponentUpdateAsyncHandler += value;
            }
            remove 
            {
                _eventStore.OnComponentUpdateAsyncHandler -= value;
            }
        }
    }
}