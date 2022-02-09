using DistributedWebCrawler.Core.Models;
using DistributedWebCrawler.Core.Interfaces;

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

        public Task NotifyCompletedAsync(RequestBase item, ComponentInfo nodeInfo, TSuccess result)
        {
            return PublishCompletedItemAsync(item, nodeInfo, result);
        }

        public Task NotifyFailedAsync(RequestBase item, ComponentInfo nodeInfo, TFailure result)
        {
            return PublishCompletedItemAsync(item, nodeInfo, result);
        }

        public Task NotifyComponentStatusUpdateAsync(ComponentInfo nodeInfo, ComponentStatus componentStatus)
        {
            return PublishAsync<ComponentStatus>(nodeInfo, componentStatus);
        }

        private Task PublishCompletedItemAsync<TResult>(RequestBase item, ComponentInfo nodeInfo, TResult result)
            where TResult : notnull        
        {
            var eventArgs = new CompletedItem<TResult>(item.Id, result);
            return PublishAsync<TResult>(nodeInfo, eventArgs);
        }

        private Task PublishAsync<TResult>(ComponentInfo nodeInfo, object result)
            where TResult : notnull
        {
            var exchangeName = _exchangeNameProvider.GetExchangeName<TResult>();
            return PublishAsync(exchangeName, nodeInfo, result);
        }

        private Task PublishAsync<TData>(string exchangeName, ComponentInfo nodeInfo, TData data)
        {
            var message = new RabbitMQMessage<TData>(nodeInfo.ComponentId, nodeInfo.NodeId, data);
            var bytes = _serializer.Serialize(message);

            _channelPool.PublishFanout(bytes, exchangeName);

            return Task.CompletedTask;
        }
    }
}