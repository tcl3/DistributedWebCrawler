using DistributedWebCrawler.Core.Components;
using DistributedWebCrawler.Core.Interfaces;
using DistributedWebCrawler.Core.Model;
using System.Threading.Tasks;

namespace DistributedWebCrawler.Core.Queue
{
    public class InMemoryEventDispatcher<TSuccess, TFailure> : IEventDispatcher<TSuccess, TFailure>
        where TSuccess : notnull
        where TFailure : notnull
    {
        private readonly InMemoryEventStore<TSuccess, TFailure> _eventStore;
        private readonly ComponentNameProvider _componentNameProvider;

        public InMemoryEventDispatcher(InMemoryEventStore<TSuccess, TFailure> eventStore,
            ComponentNameProvider componentNameProvider)
        {
            _eventStore = eventStore;
            _componentNameProvider = componentNameProvider;
        }

        public Task NotifyCompletedAsync(RequestBase item, TSuccess result)
        {
            return Notify(_eventStore.OnCompletedAsyncHandler, item, result);
        }

        public Task NotifyFailedAsync(RequestBase item, TFailure result)
        {
            return Notify(_eventStore.OnFailedAsyncHandler, item, result);
        }

        public async Task NotifyComponentStatusUpdateAsync(ComponentStatus componentStatus)
        {
            if (_eventStore.OnComponentUpdateAsyncHandler != null)
            {
                var componentName = _componentNameProvider.GetComponentNameOrDefault<TSuccess, TFailure>();
                await _eventStore.OnComponentUpdateAsyncHandler(this, new ComponentEventArgs<ComponentStatus>(componentName, componentStatus)).ConfigureAwait(false);
            }
        }

        private async Task Notify<TResult>(ItemCompletedEventHandler<TResult>? handler, RequestBase item, TResult result)
            where TResult : notnull
        {
            if (handler != null)
            {
                var componentName = _componentNameProvider.GetComponentNameOrDefault<TSuccess, TFailure>();
                await handler(this, new ItemCompletedEventArgs<TResult>(item.Id, componentName, result)).ConfigureAwait(false);
            }
        }
    }
}
