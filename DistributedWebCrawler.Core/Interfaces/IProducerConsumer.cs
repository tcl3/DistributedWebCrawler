using DistributedWebCrawler.Core.Models;
using DistributedWebCrawler.Core.Queue;
using System.Threading.Tasks;

namespace DistributedWebCrawler.Core.Interfaces
{
    public delegate Task ItemCompletedEventHandler<TResult>(object? sender, ItemCompletedEventArgs<TResult> e)
        where TResult : notnull;

    public delegate Task ItemCompletedEventHandler(object? sender, ItemCompletedEventArgs e);

    public delegate Task ItemFailedEventHandler<TFailure>(object? sender, ItemFailedEventArgs<TFailure> e)
        where TFailure : notnull, IErrorCode;

    public delegate Task ItemFailedEventHandler(object? sender, ItemFailedEventArgs e);

    public delegate Task ComponentEventHandler<TResult>(object? sender, ComponentEventArgs<TResult> e)
        where TResult : notnull;

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
