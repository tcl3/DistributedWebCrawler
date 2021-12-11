using DistributedWebCrawler.Core.Components;
using DistributedWebCrawler.Core.Interfaces;
using DistributedWebCrawler.Core.Model;
using DistributedWebCrawler.Core.Queue;

namespace DistributedWebCrawler.Extensions.RabbitMQ
{
    public class RabbitMQEventDispatcher<TSuccess, TFailure> : IEventDispatcher<TSuccess, TFailure>
        where TSuccess : notnull
        where TFailure : notnull, IErrorCode
    {
        private readonly ExchangeNameProvider<TSuccess, TFailure> _exchangeNameProvider;
        private readonly RabbitMQChannelPool _channelPool;
        private readonly ISerializer _serializer;

        public RabbitMQEventDispatcher(ExchangeNameProvider<TSuccess, TFailure> exchangeNameProvider, 
            RabbitMQChannelPool channelPool,
            ISerializer serializer)
        {
            _exchangeNameProvider = exchangeNameProvider;
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
            return PublishAsync<ComponentStatus>(componentStatus);
        }

        private Task PublishCompletedItemAsync<TResult>(RequestBase item, TResult result)
            where TResult : notnull        
        {
            var eventArgs = new CompletedItem<TResult>(item.Id, result);
            return PublishAsync<TResult>(eventArgs);
        }

        private Task PublishAsync<TResult>(object result)
            where TResult : notnull
        {
            var exchangeName = _exchangeNameProvider.GetExchangeName<TResult>();
            return PublishAsync(exchangeName, result);
        }

        private Task PublishAsync<TData>(string exchangeName, TData data)
        {
            var bytes = _serializer.Serialize(data);

            _channelPool.PublishFanout(bytes, exchangeName);

            return Task.CompletedTask;
        }
    }
}