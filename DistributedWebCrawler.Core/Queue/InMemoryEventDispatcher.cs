using DistributedWebCrawler.Core.Components;
using DistributedWebCrawler.Core.Interfaces;
using DistributedWebCrawler.Core.Model;
using System.Threading.Tasks;

namespace DistributedWebCrawler.Core.Queue
{
    public class InMemoryEventDispatcher<TSuccess, TFailure> : IEventDispatcher<TSuccess, TFailure>
    {
        private readonly InMemoryEventStore<TSuccess, TFailure> _eventStore;

        public InMemoryEventDispatcher(InMemoryEventStore<TSuccess, TFailure> eventStore)
        {
            _eventStore = eventStore;
        }

        public async Task NotifyCompletedAsync(RequestBase item, TSuccess result)
        {
            if (_eventStore.OnCompletedAsyncHandler != null)
            {
                await _eventStore.OnCompletedAsyncHandler(this, new ItemCompletedEventArgs<TSuccess>(item.Id, result)).ConfigureAwait(false);
            }
        }

        public async Task NotifyFailedAsync(RequestBase item, TFailure result)
        {
            if (_eventStore.OnFailedAsyncHandler != null)
            {
                await _eventStore.OnFailedAsyncHandler(this, new ItemCompletedEventArgs<TFailure>(item.Id, result)).ConfigureAwait(false);
            }
        }


        public async Task NotifyComponentStatusUpdateAsync(ComponentStatus componentStatus)
        {
            if (_eventStore.OnComponentUpdateAsyncHandler != null)
            {
                await _eventStore.OnComponentUpdateAsyncHandler.Invoke(this, componentStatus).ConfigureAwait(false);
            }
        }
    }
}
