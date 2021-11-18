using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace DistributedWebCrawler.Core.Interfaces
{
    public interface IProducerConsumer<TData> : IProducer<TData>, IConsumer<TData>
        where TData : class
    {

    }

    public interface IProducerConsumer<TData, TPriority> : IProducer<TData, TPriority>, IConsumer<TData>
        where TData : class
    {

    }

    public interface IProducer<TData, TPriority>
        where TData : class
    {
        void Enqueue(TData data, TPriority priority);
    }

    public interface IProducer<TData>
        where TData : class
    {
        void Enqueue(TData data);
    }

    public interface IConsumer<TData>
        where TData : class
    {       
        Task<TData> DequeueAsync();
    }
}
