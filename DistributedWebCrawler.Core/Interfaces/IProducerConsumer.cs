using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace DistributedWebCrawler.Core.Interfaces
{
    public interface IProducerConsumer<TData> : IProducer<TData>, IConsumer<TData>, IProducerConsumer
    {

    }

    public interface IProducerConsumer<TData, TPriority> : IProducer<TData, TPriority>, IConsumer<TData>, IProducerConsumer
    {

    }

    public interface IProducer<TData, TPriority>
    {
        void Enqueue(TData data, TPriority priority);
    }

    public interface IProducer<TData> : IProducerConsumer
    {
        void Enqueue(TData data);
    }

    public interface IConsumer<TData> : IProducerConsumer
    {
        bool TryDequeue([NotNullWhen(returnValue: true)] out TData? data);
    }

    public interface IProducerConsumer
    {
        int Count { get; }
    }
}
