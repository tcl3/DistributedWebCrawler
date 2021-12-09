using DistributedWebCrawler.Core.Components;
using DistributedWebCrawler.Core.Interfaces;
using DistributedWebCrawler.Core.Model;
using DistributedWebCrawler.Core.Queue;

namespace DistributedWebCrawler.Extensions.RabbitMQ
{
    public class RabbitMQEventDispatcher<TSuccess, TFailure> : IEventDispatcher<TSuccess, TFailure>
        where TSuccess : notnull
        where TFailure : notnull
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
            return PublishCompletedItemAsync(item, result);
        }

        public Task NotifyFailedAsync(RequestBase item, TFailure result)
        {
            return PublishCompletedItemAsync(item, result);
        }

        public Task NotifyComponentStatusUpdateAsync(ComponentStatus componentStatus)
        {
            return PublishAsync(componentStatus);
        }

        private Task PublishCompletedItemAsync<TResult>(RequestBase item, TResult result)
            where TResult : notnull        
        {
            var eventArgs = new CompletedItem<TResult>(item.Id, result);
            return PublishAsync(eventArgs);
        }

        private Task PublishAsync<TResult>(TResult result)
            where TResult : notnull
        {
            var queueName = _queueNameProvider.GetQueueName<TResult>();
            return PublishAsync(queueName, result);
        }

        private Task PublishAsync<TData>(string queueName, TData data)
        {
            var bytes = _serializer.Serialize(data);

            _channelPool.Publish(bytes, RabbitMQConstants.ProducerConsumer.ExchangeName, queueName);

            return Task.CompletedTask;
        }
    }
}