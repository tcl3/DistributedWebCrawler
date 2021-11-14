using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace DistributedWebCrawler.Core.Interfaces
{
    public interface IProducerConsumer<TData> : IProducer<TData>, IConsumer<TData>
    {

    }

    public interface IProducerConsumer<TData, TPriority> : IProducer<TData, TPriority>, IConsumer<TData>
    {

    }

    public interface IProducer<TData, TPriority>
    {
        void Enqueue(TData data, TPriority priority);
    }

    public interface IProducer<TData>
    {
        void Enqueue(TData data);
    }

    public interface IConsumer<TData>
    {       
        Task<TData> DequeueAsync();
    }
}
