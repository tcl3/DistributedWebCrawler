using DistributedWebCrawler.Core.Components;
using DistributedWebCrawler.Core.Interfaces;
using DistributedWebCrawler.Core.Model;
using DistributedWebCrawler.Core.Queue;

namespace DistributedWebCrawler.Extensions.RabbitMQ
{
    public class RabbitMQEventDispatcher<TSuccess, TFailure> : IEventDispatcher<TSuccess, TFailure>
    {
        private readonly QueueNameProvider<TSuccess, TFailure> _queueNameProvider;
        private readonly RabbitMQChannelPool _channelPool;
        private readonly ISerializer _serializer;

        public RabbitMQEventDispatcher(QueueNameProvider<TSuccess, TFailure> queueNameProvider, 
            RabbitMQChannelPool channelPool,
            ISerializer serializer)
        {
            _queueNameProvider = queueNameProvider;
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
            var queueName = _queueNameProvider.GetQueueName<TResult>();
            return PublishAsync(queueName, eventArgs);
        }

        private Task PublishAsync<TData>(string queueName, TData data)
        {
            var bytes = _serializer.Serialize(data);

            _channelPool.Publish(bytes, RabbitMQConstants.ProducerConsumer.ExchangeName, queueName);

            return Task.CompletedTask;
        }

        public Task NotifyComponentStatusUpdateAsync(ComponentStatus componentStatus)
        {
            var queueName = _queueNameProvider.GetQueueName<ComponentStatus>();
            return PublishAsync(queueName, componentStatus);
        }
    }
}