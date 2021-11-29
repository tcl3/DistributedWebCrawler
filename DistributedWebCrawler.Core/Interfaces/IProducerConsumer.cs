using DistributedWebCrawler.Core.Model;
using DistributedWebCrawler.Core.Queue;
using System;
using System.Threading.Tasks;

namespace DistributedWebCrawler.Core.Interfaces
{
    public delegate Task ItemCompletedEventHandler<TResult>(object? sender, ItemCompletedEventArgs<TResult> e);
    public delegate Task ItemCompletedEventHandler(object? sender, ItemCompletedEventArgs e);

    public interface IProducerConsumer<TRequest, TResult>
        : IProducer<TRequest, TResult>, IConsumer<TRequest, TResult>
        where TRequest : RequestBase
    {

    }

    public interface IProducer<TRequest, TResult> : IProducerConsumer
        where TRequest : RequestBase
    {
        void Enqueue(TRequest data);
    }

    public interface IConsumer<TRequest, TResult> : IProducerConsumer
        where TRequest : RequestBase
    {
        Task<TRequest> DequeueAsync();
    }

    public interface IProducerConsumer
    {
        int Count { get; }
    }
}
