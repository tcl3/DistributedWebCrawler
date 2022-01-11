using RabbitMQ.Client;

namespace DistributedWebCrawler.Extensions.RabbitMQ.Interfaces
{
    public interface IPooledChannel : IDisposable
    {
        IModel Channel { get; }
    }
}
