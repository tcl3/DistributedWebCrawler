using DistributedWebCrawler.Core.Model;
using DistributedWebCrawler.Core.Queue;
using System;
using System.Threading.Tasks;

namespace DistributedWebCrawler.Core.Interfaces
{
    public interface IProducerConsumer<TData> : IProducer<TData>, IConsumer<TData>
        where TData : RequestBase
    {

    }

    public interface IProducerConsumer<TData, TPriority> : IProducer<TData, TPriority>, IConsumer<TData>
        where TData : RequestBase
    {

    }

    public interface IProducer<TData, TPriority>
        where TData : RequestBase
    {
        void Enqueue(TData data, TPriority priority);
    }

    public interface IProducer<TData>
        where TData : RequestBase
    {
        void Enqueue(TData data);

        event EventHandler<ItemCompletedEventArgs> OnCompleted;
    }

    public interface IConsumer<TData>
        where TData : RequestBase
    {       
        Task<TData> DequeueAsync();
        void NotifyCompleted(TData item);
    }
}
