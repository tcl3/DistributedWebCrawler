using DistributedWebCrawler.Core.Components;
using DistributedWebCrawler.Core.Interfaces;
using DistributedWebCrawler.Core.Queue;
using DistributedWebCrawler.Extensions.RabbitMQ.Interfaces;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Collections.Concurrent;
using System.Runtime.Serialization;

namespace DistributedWebCrawler.Extensions.RabbitMQ
{
    public class RabbitMQEventReceiver<TSuccess, TFailure> : IEventReceiver<TSuccess, TFailure>
    {
        private readonly InMemoryEventStore<TSuccess, TFailure> _eventStore;
        private readonly QueueNameProvider<TSuccess, TFailure> _queueNameProvider;
        private readonly IPersistentConnection _connection;
        private readonly ISerializer _serializer;
        private readonly ILogger _logger;
        
        private readonly ConcurrentDictionary<string, IModel> _notifierReceiveChannelLookup;

        public RabbitMQEventReceiver(InMemoryEventStore<TSuccess, TFailure> eventStore,
            QueueNameProvider<TSuccess, TFailure> queueNameProvider,
            IPersistentConnection connection, 
            ISerializer serializer, 
            ILogger<RabbitMQEventReceiver<TSuccess, TFailure>> logger)
        {
            _eventStore = eventStore;
            _queueNameProvider = queueNameProvider;
            _connection = connection;
            _serializer = serializer;
            _logger = logger;

            _notifierReceiveChannelLookup = new();
        }

        public event ItemCompletedEventHandler<TFailure> OnFailedAsync
        {
            add
            {
                StartOnFailedNotifier();
                _eventStore.OnFailedAsyncHandler += value;
            }

            remove
            {
                _eventStore.OnFailedAsyncHandler -= value;
            }
        }

        public event ItemCompletedEventHandler<TSuccess> OnCompletedAsync
        {
            add
            {
                StartOnCompletedNotifier(); 
                _eventStore.OnCompletedAsyncHandler += value;
            }

            remove
            {
                _eventStore.OnCompletedAsyncHandler -= value;
            }
        }

        event ItemCompletedEventHandler IEventReceiver.OnCompletedAsync
        {
            add
            {
                StartOnCompletedNotifier();
                _eventStore.OnCompletedAsync += value;
            }

            remove
            {
                _eventStore.OnCompletedAsync -= value;
            }
        }

        event ItemCompletedEventHandler IEventReceiver.OnFailedAsync
        {
            add
            {
                StartOnFailedNotifier();
                _eventStore.OnFailedAsync += value;
            }

            remove
            {
                _eventStore.OnFailedAsync -= value;
            }
        }

        public event Core.AsyncEventHandler<ComponentStatus> OnComponentUpdateAsync
        {
            add
            {
                StartComponentStatusUpdateNotifier();
                _eventStore.OnComponentUpdateAsyncHandler += value;
            }
            remove
            {
                _eventStore.OnComponentUpdateAsyncHandler -= value;
            }
        }

        private void StartOnFailedNotifier()
        {
            var receiveCallback = OnNotificationReceived<ItemCompletedEventArgs<TFailure>, TFailure>((obj, args) => _eventStore.OnFailedAsyncHandler?.Invoke(obj, args) ?? Task.CompletedTask);
            StartNotifierConsumer<TFailure>(receiveCallback);
        }

        private void StartOnCompletedNotifier()
        {
            var receiveCallback = OnNotificationReceived<ItemCompletedEventArgs<TSuccess>, TSuccess>((obj, args) => _eventStore.OnCompletedAsyncHandler?.Invoke(obj, args) ?? Task.CompletedTask);
            StartNotifierConsumer<TSuccess>(receiveCallback);
        }

        private void StartComponentStatusUpdateNotifier()
        {
            var receiveCallback = OnNotificationReceived<ComponentStatus, ComponentStatus>((obj, args) => _eventStore.OnComponentUpdateAsyncHandler?.Invoke(obj, args) ?? Task.CompletedTask);
            StartNotifierConsumer<ComponentStatus>(receiveCallback);
        }

        private void StartNotifierConsumer<TData>(AsyncEventHandler<BasicDeliverEventArgs> receiveCallback)
        {
            var queueName = _queueNameProvider.GetQueueName<TData>();
            StartNotifierConsumer(queueName, receiveCallback);
        }

        private void StartNotifierConsumer(string queueName, AsyncEventHandler<BasicDeliverEventArgs> receiveCallback)
        {
            _notifierReceiveChannelLookup.GetOrAdd(queueName, k =>
            {
                if (!_connection.IsConnected)
                {
                    _connection.TryConnect();
                }

                return _connection.StartConsumer(queueName, receiveCallback, _logger);
            });
        }

        private AsyncEventHandler<BasicDeliverEventArgs> OnNotificationReceived<TArgs, TData>(Func<object?, TArgs, Task> handler)
        {
            var queueName = _queueNameProvider.GetQueueName<TData>();
            return OnNotificationReceived(queueName, handler);
        }

        private AsyncEventHandler<BasicDeliverEventArgs> OnNotificationReceived<TArgs>(string queueName, Func<object?, TArgs, Task> handler)
        {
            return async (model, ea) =>
            {
                var eventArgs = _serializer.Deserialize<TArgs>(ea.Body.Span);

                if (eventArgs == null)
                {
                    throw new SerializationException($"Failed to deserialize event data of type {typeof(TArgs).Name}");
                }

                if (_eventStore.OnCompletedAsyncHandler != null)
                {
                    await handler(this, eventArgs).ConfigureAwait(false);
                }

                if (!_notifierReceiveChannelLookup.TryGetValue(queueName, out var channel))
                {
                    throw new InvalidOperationException($"Channel for queue: {queueName} not found");
                }


                channel.BasicAck(ea.DeliveryTag, multiple: false);
            };
        }
    }
}