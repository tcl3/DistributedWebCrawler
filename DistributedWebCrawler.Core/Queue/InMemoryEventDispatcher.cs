using DistributedWebCrawler.Core.Interfaces;
using DistributedWebCrawler.Core.Model;
using System.Threading.Tasks;

namespace DistributedWebCrawler.Core.Queue
{
    public class InMemoryEventDispatcher<TRequest, TResult>
        : IEventDispatcher<TRequest, TResult>
        where TRequest : RequestBase
    {
        private readonly InMemoryEventStore<TResult> _eventStore;

        public InMemoryEventDispatcher(InMemoryEventStore<TResult> eventStore)
        {
            _eventStore = eventStore;
        }

        public async Task NotifyCompletedAsync(TRequest item, TaskStatus status, TResult? result)
        {
            if (_eventStore.OnCompletedAsyncHandler != null)
            {
                await _eventStore.OnCompletedAsyncHandler.Invoke(this, new ItemCompletedEventArgs<TResult>(item.Id, status) { Result = result }).ConfigureAwait(false);
            }
        }
    }
}
