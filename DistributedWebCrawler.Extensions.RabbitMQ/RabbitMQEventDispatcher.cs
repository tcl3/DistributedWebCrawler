using DistributedWebCrawler.Core.Interfaces;
using DistributedWebCrawler.Core.Model;
using DistributedWebCrawler.Core.Queue;

namespace DistributedWebCrawler.Extensions.RabbitMQ
{
    public class RabbitMQEventDispatcher<TSuccess, TFailure> : IEventDispatcher<TSuccess, TFailure>
    {
        private readonly RabbitMQChannelPool _channelPool;
        private readonly ISerializer _serializer;
        
        private const string NotifierQueueSuffix = "-Notifier";

        public RabbitMQEventDispatcher(RabbitMQChannelPool channelPool, ISerializer serializer)
        {
            _channelPool = channelPool;
            _serializer = serializer;
        }

        public Task NotifyCompletedAsync(RequestBase item, TSuccess result)
        {

            return PublishAsync(item, result);
        }

        public Task NotifyFailedAsync(RequestBase item, TFailure result)
        {
            return PublishAsync(item, result);
        }

        private Task PublishAsync<TResult>(RequestBase item, TResult result)
        {
            var eventArgs = new ItemCompletedEventArgs<TResult>(item.Id, result);
            
            var bytes = _serializer.Serialize(eventArgs);

            var queueName = typeof(TResult).Name + NotifierQueueSuffix;

            _channelPool.Publish(bytes, RabbitMQConstants.ProducerConsumer.ExchangeName, queueName);

            return Task.CompletedTask;
        }
    }
}
