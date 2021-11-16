using RabbitMQ.Client;

namespace DistributedWebCrawler.Extensions.RabbitMQ.Interfaces
{
    public interface IPersistentConnection : IDisposable
    {
        bool IsConnected { get; }
        IModel CreateModel();
        bool TryConnect();
    }
}
