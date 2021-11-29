using DistributedWebCrawler.Core.Interfaces;
using DistributedWebCrawler.Core.Model;
using DistributedWebCrawler.Core.Queue;
using DistributedWebCrawler.Extensions.RabbitMQ.Interfaces;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Runtime.Serialization;

namespace DistributedWebCrawler.Extensions.RabbitMQ
{
    public class RabbitMQEventReceiver<TRequest, TResult>
        : IEventReceiver<TRequest, TResult>
        where TRequest : RequestBase
    {
        private static readonly object _syncRoot = new();

        private static readonly string NotifierQueueName = typeof(TRequest).Name + "-Notifier";

        private readonly IPersistentConnection _connection;
        private readonly ISerializer _serializer;
        private readonly ILogger<RabbitMQEventReceiver<TRequest, TResult>> _logger;
        
        private IModel? _notifierReceiveChannel;
        private ItemCompletedEventHandler<TResult>? _onCompletedAsync = (_, _) => Task.CompletedTask;

        public RabbitMQEventReceiver(IPersistentConnection connection, ISerializer serializer, 
            ILogger<RabbitMQEventReceiver<TRequest, TResult>> logger)
        {
            _connection = connection;
            _serializer = serializer;
            _logger = logger;
        }

        public event ItemCompletedEventHandler<TResult> OnCompletedAsync
        {
            add
            {
                if (_notifierReceiveChannel == null)
                {
                    lock (_syncRoot)
                    {
                        if (_notifierReceiveChannel == null)
                        {
                            if (!_connection.IsConnected)
                            {
                                _connection.TryConnect();
                            }
                            _notifierReceiveChannel = _connection.StartConsumer(NotifierQueueName, OnNotificationReceived, _logger);
                        }
                    }
                }

                _onCompletedAsync += value;
            }

            remove
            {
                _onCompletedAsync -= value;
            }
        }

        private async Task OnNotificationReceived(object? model, BasicDeliverEventArgs ea)
        {
            var eventArgs = _serializer.Deserialize<ItemCompletedEventArgs<TResult>>(ea.Body.Span);

            if (eventArgs == null)
            {
                throw new SerializationException("Failed to deserialize OnCompleted event data");
            }

            if (_onCompletedAsync != null)
            {
                await _onCompletedAsync.Invoke(this, eventArgs).ConfigureAwait(false);
            }

            _notifierReceiveChannel?.BasicAck(ea.DeliveryTag, multiple: false);
        }
    }
}
