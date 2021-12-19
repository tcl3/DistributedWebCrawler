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
        where TFailure : notnull, IErrorCode
    {
        private readonly InMemoryEventStore<TSuccess, TFailure> _eventStore;
        private readonly ExchangeNameProvider<TSuccess, TFailure> _exchangeNameProvider;
        private readonly ComponentNameProvider _componentNameProvider;
        private readonly IPersistentConnection _connection;
        private readonly ISerializer _serializer;
        private readonly ILogger _logger;

        private readonly ConcurrentDictionary<string, IModel> _notifierReceiveChannelLookup;

        public RabbitMQEventReceiver(InMemoryEventStore<TSuccess, TFailure> eventStore,
            ExchangeNameProvider<TSuccess, TFailure> exchangeNameProvider,
            ComponentNameProvider componentNameProvider,
            IPersistentConnection connection,
            ISerializer serializer,
            ILogger<RabbitMQEventReceiver<TSuccess, TFailure>> logger)
        {
            _eventStore = eventStore;
            _exchangeNameProvider = exchangeNameProvider;
            _componentNameProvider = componentNameProvider;
            _connection = connection;
            _serializer = serializer;
            _logger = logger;

            _notifierReceiveChannelLookup = new();
        }

        public event ItemFailedEventHandler<TFailure> OnFailedAsync
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

        event ItemFailedEventHandler IEventReceiver.OnFailedAsync
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
            var argsFactory = (CompletedItem<TFailure> data, string componentName) => new ItemFailedEventArgs<TFailure>(data.Id, componentName, data.Result);
            var handler = (object? obj, ItemFailedEventArgs<TFailure> args) => _eventStore.OnFailedAsyncHandler?.Invoke(obj, args);
            var receiveCallback = OnNotificationReceived<ItemFailedEventArgs<TFailure>, CompletedItem<TFailure>, TFailure>(handler, argsFactory);
            StartNotifierConsumer<TFailure>(receiveCallback);
        }

        private void StartOnCompletedNotifier()
        {
            var argsFactory = (CompletedItem<TSuccess> data, string componentName) => new ItemCompletedEventArgs<TSuccess>(data.Id, componentName, data.Result);
            var handler = (object? obj, ItemCompletedEventArgs<TSuccess> args) => _eventStore.OnCompletedAsyncHandler?.Invoke(obj, args);
            var receiveCallback = OnNotificationReceived<ItemCompletedEventArgs<TSuccess>, CompletedItem<TSuccess>, TSuccess>(handler, argsFactory);
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
            var exchangeName = _exchangeNameProvider.GetExchangeName<TData>();

            _notifierReceiveChannelLookup.GetOrAdd(exchangeName, k =>
            {
                if (!_connection.IsConnected)
                {
                    _connection.TryConnect();
                }

                return _connection.StartConsumerForNotifier(exchangeName, receiveCallback, _logger);
            });
        }

        private BasicDeliverAsyncEventHandler OnNotificationReceived<TArgs, TData, TResult>(Func<object?, TArgs, Task?> handler, Func<TData, string, TArgs> argsFactory)
            where TArgs: EventArgs
            where TResult : notnull
        {
            var exchangeName = _exchangeNameProvider.GetExchangeName<TResult>();
            return OnNotificationReceived(exchangeName, handler, argsFactory);
        }

        private BasicDeliverAsyncEventHandler OnNotificationReceived<TArgs, TData>(string exchangeName, Func<object?, TArgs, Task?> handler, Func<TData, string, TArgs> argsFactory)
            where TArgs : EventArgs
        {
            return async (model, ea) =>
            {
                var data = _serializer.Deserialize<TData>(ea.Body.Span);

                if (data == null)
                {
                    throw new SerializationException($"Failed to deserialize event data of type {typeof(TData).Name}");
                }

                if (handler != null)
                {
                    var componentName = _componentNameProvider.GetComponentNameOrDefault<TSuccess, TFailure>();
                    var eventArgs = argsFactory(data, componentName);
                    var handlerTask = handler(this, eventArgs);
                    if (handlerTask != null)
                    {
                        await handlerTask.ConfigureAwait(false);
                    }                    
                }

                if (!_notifierReceiveChannelLookup.TryGetValue(exchangeName, out var channel))
                {
                    throw new InvalidOperationException($"Channel for exchange: {exchangeName} not found");
                }


                channel.BasicAck(ea.DeliveryTag, multiple: false);
            };
        }
    }
}