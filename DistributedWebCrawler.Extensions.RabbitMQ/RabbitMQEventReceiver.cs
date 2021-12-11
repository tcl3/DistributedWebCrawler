using DistributedWebCrawler.Core;
using DistributedWebCrawler.Core.Components;
using DistributedWebCrawler.Core.Interfaces;
using DistributedWebCrawler.Core.Queue;
using DistributedWebCrawler.Extensions.RabbitMQ.Interfaces;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System.Collections.Concurrent;
using System.Runtime.Serialization;
using BasicDeliverAsyncEventHandler = RabbitMQ.Client.Events.AsyncEventHandler<RabbitMQ.Client.Events.BasicDeliverEventArgs>;

namespace DistributedWebCrawler.Extensions.RabbitMQ
{
    public class RabbitMQEventReceiver<TSuccess, TFailure> : IEventReceiver<TSuccess, TFailure>
        where TSuccess : notnull
        where TFailure : notnull
    {
        private readonly InMemoryEventStore<TSuccess, TFailure> _eventStore;
        private readonly QueueNameProvider<TSuccess, TFailure> _queueNameProvider;
        private readonly ComponentNameProvider _componentNameProvider;
        private readonly IPersistentConnection _connection;
        private readonly ISerializer _serializer;
        private readonly ILogger _logger;

        private readonly ConcurrentDictionary<string, IModel> _notifierReceiveChannelLookup;

        public RabbitMQEventReceiver(InMemoryEventStore<TSuccess, TFailure> eventStore,
            QueueNameProvider<TSuccess, TFailure> queueNameProvider,
            ComponentNameProvider componentNameProvider,
            IPersistentConnection connection,
            ISerializer serializer,
            ILogger<RabbitMQEventReceiver<TSuccess, TFailure>> logger)
        {
            _eventStore = eventStore;
            _queueNameProvider = queueNameProvider;
            _componentNameProvider = componentNameProvider;
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

        public event ComponentEventHandler<ComponentStatus> OnComponentUpdateAsync
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
            var receiveCallback = OnItemCompletedNotificationReceived<TFailure>((obj, args) => _eventStore.OnFailedAsyncHandler?.Invoke(obj, args));
            StartNotifierConsumer<TFailure>(receiveCallback);
        }

        private void StartOnCompletedNotifier()
        {
            var receiveCallback = OnItemCompletedNotificationReceived<TSuccess>((obj, args) => _eventStore.OnCompletedAsyncHandler?.Invoke(obj, args));
            StartNotifierConsumer<TSuccess>(receiveCallback);
        }

        private void StartComponentStatusUpdateNotifier()
        {
            var argsFactory = (ComponentStatus data, string componentName) => new ComponentEventArgs<ComponentStatus>(componentName, data);
            var handler = (object? obj, ComponentEventArgs<ComponentStatus> args) => _eventStore.OnComponentUpdateAsyncHandler?.Invoke(obj, args);
            var receiveCallback = OnNotificationReceived<ComponentEventArgs<ComponentStatus>, ComponentStatus, ComponentStatus>(handler, argsFactory);
            StartNotifierConsumer<ComponentStatus>(receiveCallback);
        }

        private void StartNotifierConsumer<TData>(BasicDeliverAsyncEventHandler receiveCallback)
        {
            var queueName = _queueNameProvider.GetQueueName<TData>();
            StartNotifierConsumer(queueName, receiveCallback);
        }

        private void StartNotifierConsumer(string queueName, BasicDeliverAsyncEventHandler receiveCallback)
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

        private BasicDeliverAsyncEventHandler OnItemCompletedNotificationReceived<TResult>(Func<object?, ItemCompletedEventArgs<TResult>, Task?> handler)
            where TResult : notnull
        {
            var argsFactory = (CompletedItem<TResult> data, string componentName) => new ItemCompletedEventArgs<TResult>(data.Id, componentName, data.Result);
            return OnNotificationReceived<ItemCompletedEventArgs<TResult>, CompletedItem<TResult>, TResult>(handler, argsFactory);
        }

        private BasicDeliverAsyncEventHandler OnNotificationReceived<TArgs, TData, TResult>(Func<object?, TArgs, Task?> handler, Func<TData, string, TArgs> argsFactory)
            where TArgs: ComponentEventArgs
            where TResult : notnull
        {
            var queueName = _queueNameProvider.GetQueueName<TResult>();
            return OnNotificationReceived(queueName, handler, argsFactory);
        }

        private BasicDeliverAsyncEventHandler OnNotificationReceived<TArgs, TData>(string queueName, Func<object?, TArgs, Task?> handler, Func<TData, string, TArgs> argsFactory)
            where TArgs : ComponentEventArgs
        {
            return async (model, ea) =>
            {
                var data = _serializer.Deserialize<TData>(ea.Body.Span);

                if (data == null)
                {
                    throw new SerializationException($"Failed to deserialize event data of type {typeof(TData).Name}");
                }

                if (_eventStore.OnCompletedAsyncHandler != null)
                {
                    var componentName = _componentNameProvider.GetComponentNameOrDefault<TSuccess, TFailure>();
                    var eventArgs = argsFactory(data, componentName);
                    var handlerTask = handler(this, eventArgs);
                    if (handlerTask != null)
                    {
                        await handlerTask.ConfigureAwait(false);
                    }                    
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