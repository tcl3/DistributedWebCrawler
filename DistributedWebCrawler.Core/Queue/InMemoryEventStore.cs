using DistributedWebCrawler.Core.Interfaces;

namespace DistributedWebCrawler.Core.Queue
{
    public class InMemoryEventStore<TResult>
    {
        public ItemCompletedEventHandler<TResult>? OnCompletedAsyncHandler { get; set; }
    }
}
