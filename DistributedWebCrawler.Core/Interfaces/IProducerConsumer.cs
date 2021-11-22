using DistributedWebCrawler.Core.Model;
using DistributedWebCrawler.Core.Queue;
using System;
using System.Threading.Tasks;

namespace DistributedWebCrawler.Core.Interfaces
{
    public interface IProducerConsumer<TRequest, TResult>
        : IProducer<TRequest, TResult>, IConsumer<TRequest, TResult>
        where TRequest : RequestBase
    {

    }

    public interface IProducer<TRequest, TResult> : IProducerConsumer
        where TRequest : RequestBase
    {
        void Enqueue(TRequest data);

        event EventHandler<ItemCompletedEventArgs<TResult>> OnCompleted;
    }

    public interface IConsumer<TRequest, TResult> : IProducerConsumer
        where TRequest : RequestBase
    {
        Task<TRequest> DequeueAsync();
        void NotifyCompleted(TRequest item, TaskStatus status, TResult? result);
    }

    public interface IProducerConsumer
    {
        int Count { get; }
    }
}
