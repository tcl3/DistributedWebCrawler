using DistributedWebCrawler.Core.Interfaces;
using DistributedWebCrawler.Core.Model;
using DistributedWebCrawler.Core.Queue;

namespace DistributedWebCrawler.Extensions.RabbitMQ
{
    public class RabbitMQEventDispatcher<TRequest, TResult> : IEventDispatcher<TRequest, TResult> 
        where TRequest : RequestBase
    {
        private readonly RabbitMQChannelPool _channelPool;

        private static readonly string NotifierQueueName = typeof(TRequest).Name + "-Notifier";

        public RabbitMQEventDispatcher(RabbitMQChannelPool channelPool)
        {
            _channelPool = channelPool;
        }

        public Task NotifyCompletedAsync(TRequest item, TaskStatus status, TResult? result)
        {
            var eventArgs = new ItemCompletedEventArgs<TResult>(item.Id, status) { Result = result };

            _channelPool.Publish(eventArgs, RabbitMQConstants.ProducerConsumer.ExchangeName, NotifierQueueName);

            return Task.CompletedTask;
        }
    }
}
