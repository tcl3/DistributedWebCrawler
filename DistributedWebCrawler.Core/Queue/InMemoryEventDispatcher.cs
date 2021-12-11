using DistributedWebCrawler.Core.Components;
using DistributedWebCrawler.Core.Interfaces;
using DistributedWebCrawler.Core.Model;
using System.Threading.Tasks;

namespace DistributedWebCrawler.Core.Queue
{
    public class InMemoryEventDispatcher<TSuccess, TFailure> : IEventDispatcher<TSuccess, TFailure>
        where TSuccess : notnull
        where TFailure : notnull, IErrorCode
    {
        private readonly InMemoryEventStore<TSuccess, TFailure> _eventStore;
        private readonly ComponentNameProvider _componentNameProvider;

        public InMemoryEventDispatcher(InMemoryEventStore<TSuccess, TFailure> eventStore,
            ComponentNameProvider componentNameProvider)
        {
            _eventStore = eventStore;
            _componentNameProvider = componentNameProvider;
        }

        public async Task NotifyCompletedAsync(RequestBase item, TSuccess result)
        {
            if (_eventStore.OnCompletedAsyncHandler != null)
            {
                var componentName = _componentNameProvider.GetComponentNameOrDefault<TSuccess, TFailure>();
                await _eventStore.OnCompletedAsyncHandler(this, new ItemCompletedEventArgs<TSuccess>(item.Id, componentName, result)).ConfigureAwait(false);
            }
        }

        public async Task NotifyFailedAsync(RequestBase item, TFailure result)
        {
            if (_eventStore.OnFailedAsyncHandler != null)
            {
                var componentName = _componentNameProvider.GetComponentNameOrDefault<TSuccess, TFailure>();
                await _eventStore.OnFailedAsyncHandler(this, new ItemFailedEventArgs<TFailure>(item.Id, componentName, result)).ConfigureAwait(false);
            }
        }

        public async Task NotifyComponentStatusUpdateAsync(ComponentStatus componentStatus)
        {
            if (_eventStore.OnComponentUpdateAsyncHandler != null)
            {
                var componentName = _componentNameProvider.GetComponentNameOrDefault<TSuccess, TFailure>();
                await _eventStore.OnComponentUpdateAsyncHandler(this, new ComponentEventArgs<ComponentStatus>(componentName, componentStatus)).ConfigureAwait(false);
            }
        }
    }
}