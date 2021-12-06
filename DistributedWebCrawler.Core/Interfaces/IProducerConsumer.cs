using DistributedWebCrawler.Core.Model;
using DistributedWebCrawler.Core.Queue;
using System.Threading.Tasks;

namespace DistributedWebCrawler.Core.Interfaces
{
    public delegate Task ItemCompletedEventHandler<TResult>(object? sender, ItemCompletedEventArgs<TResult> e);
    public delegate Task ItemCompletedEventHandler(object? sender, ItemCompletedEventArgs e);

    public interface IProducerConsumer<TRequest>
        : IProducer<TRequest>, IConsumer<TRequest>
        where TRequest : RequestBase
    {

    }

    public interface IProducer<TRequest> : IProducerConsumer
        where TRequest : RequestBase
    {
        void Enqueue(TRequest data);
    }

    public interface IConsumer<TRequest> : IProducerConsumer
        where TRequest : RequestBase
    {
        Task<TRequest> DequeueAsync();
    }

    public interface IProducerConsumer
    {
        int Count { get; }
    }
}
