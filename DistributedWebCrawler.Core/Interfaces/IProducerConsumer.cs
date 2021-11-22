using DistributedWebCrawler.Core.Model;
using DistributedWebCrawler.Core.Queue;
using System;
using System.Threading.Tasks;

namespace DistributedWebCrawler.Core.Interfaces
{
    public interface IProducerConsumer<TRequest> : IProducer<TRequest>, IConsumer<TRequest>
        where TRequest : RequestBase
    {

    }

    public interface IProducerConsumer<TRequest, TPriority> : IProducer<TRequest, TPriority>, IConsumer<TRequest>
        where TRequest : RequestBase
    {

    }

    public interface IProducer<TRequest, TPriority> : IProducerConsumer
        where TRequest : RequestBase
    {
        void Enqueue(TRequest data, TPriority priority);
    }

    public interface IProducer<TRequest> : IProducerConsumer
        where TRequest : RequestBase
    {
        void Enqueue(TRequest data);

        event EventHandler<ItemCompletedEventArgs> OnCompleted;
    }

    public interface IConsumer<TRequest> : IProducerConsumer
        where TRequest : RequestBase
    {       
        Task<TRequest> DequeueAsync();
        void NotifyCompleted(TRequest item);
    }

    public interface IProducerConsumer
    {
        int Count { get; }
    }
}
