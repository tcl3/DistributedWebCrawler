using DistributedWebCrawler.Core.Interfaces;
using DistributedWebCrawler.Core.Model;
using DistributedWebCrawler.Core.Queue;
using DistributedWebCrawler.Extensions.RabbitMQ.Interfaces;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace DistributedWebCrawler.Extensions.RabbitMQ
{
    public class RabbitMQProducerConsumer<TRequest, TResult> : IProducerConsumer<TRequest, TResult>
        where TRequest : RequestBase
    {
        private readonly IPersistentConnection _connection;
        private readonly ILogger<RabbitMQProducerConsumer<TRequest, TResult>> _logger;
        private readonly RetryPolicy _retryPolicy;

        private static readonly string ConsumerQueueName = typeof(TRequest).Name;
        private static readonly string NotifierQueueName = ConsumerQueueName + "-Notifier";

        private IModel? _producerReceiveChannel;
        private IModel? _notifierReceiveChannel;

        private readonly ConcurrentQueue<TaskCompletionSource<TRequest>> _taskQueue;
        private readonly SemaphoreSlim _messageSemaphore;
        private readonly ConcurrentDictionary<int, IModel> _channelPool;

        private static readonly object _syncRoot = new();

        private EventHandler<ItemCompletedEventArgs<TResult>>? _onCompleted = (_, _) => { };

        public int Count
        {
            get
            {
                if (!_connection.IsConnected)
                {
                    _connection.TryConnect();
                }

                var channel = GetChannelFromPool();

                var result = channel.QueueDeclare(queue: ConsumerQueueName,
                                     durable: false,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);
                
                return result.MessageCount <= int.MaxValue ? (int)result.MessageCount : int.MaxValue;
            }
        }

        public event EventHandler<ItemCompletedEventArgs<TResult>>? OnCompleted
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
                            _notifierReceiveChannel = StartConumer(NotifierQueueName, OnNotificationReceived);
                        }
                    }
                }

                _onCompleted += value;
            }

            remove
            {
                _onCompleted -= value;
            }
        }

        public RabbitMQProducerConsumer(IPersistentConnection connection, 
            ILogger<RabbitMQProducerConsumer<TRequest, TResult>> logger)
        {
            _connection = connection;
            _logger = logger;

            _retryPolicy = Policy.Handle<BrokerUnreachableException>()
               .Or<AlreadyClosedException>()
               .Or<SocketException>()
               .WaitAndRetry(RabbitMQConstants.ProducerConsumer.RetryCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), (ex, time) =>
               {
                   _logger.LogWarning(ex, "Could not publish event {Timeout}s ({ExceptionMessage})", $"{time.TotalSeconds:n1}", ex.Message);
               });

            _taskQueue = new();
            _messageSemaphore = new SemaphoreSlim(0);
            _channelPool = new();            
        }

        public Task<TRequest> DequeueAsync()
        {
            if (!_connection.IsConnected)
            {
                _connection.TryConnect();
            }

            if (_producerReceiveChannel == null)
            {
                lock (_syncRoot)
                {
                    if (_producerReceiveChannel == null) _producerReceiveChannel = StartConumer(ConsumerQueueName, OnQueueItemReceived);
                }
            }

            var tcs = new TaskCompletionSource<TRequest>();
            _taskQueue.Enqueue(tcs);
            _messageSemaphore.Release();

            return tcs.Task;
        }
        private async Task OnQueueItemReceived(object? model, BasicDeliverEventArgs ea)
        {
            _logger.LogDebug($"Message received: '{Encoding.UTF8.GetString(ea.Body.Span)}'");

            var queueItem = JsonSerializer.Deserialize<TRequest>(ea.Body.Span);

            if (queueItem == null)
            {
                throw new JsonException("Failed to deserialize queue item");
            }

            TaskCompletionSource<TRequest> tcs;
            while (!_taskQueue.TryDequeue(out tcs))
            {
                await _messageSemaphore.WaitAsync().ConfigureAwait(false);
            }

            tcs.SetResult(queueItem);
            _producerReceiveChannel?.BasicAck(ea.DeliveryTag, multiple: false);
        }

        private Task OnNotificationReceived(object? model, BasicDeliverEventArgs ea)
        {
            var eventArgs = JsonSerializer.Deserialize<ItemCompletedEventArgs<TResult>>(ea.Body.Span);

            if (eventArgs == null)
            {
                throw new JsonException("Failed to deserialize OnCompleted event data");
            }
            
            _onCompleted?.Invoke(this, eventArgs);

            _notifierReceiveChannel?.BasicAck(ea.DeliveryTag, multiple: false);
            
            return Task.CompletedTask;
        }

        private IModel StartConumer(string? queueName, AsyncEventHandler<BasicDeliverEventArgs> receiveCallback)
        {
            var channel = _connection.CreateModel();

            channel.ExchangeDeclare(exchange: RabbitMQConstants.ProducerConsumer.ExchangeName, type: "direct");
            var x = channel.QueueDeclare(queue: queueName,
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);
            
            channel.QueueBind(queue: queueName, 
                exchange: RabbitMQConstants.ProducerConsumer.ExchangeName, 
                routingKey: queueName);

            channel.CallbackException += (sender, ea) =>
            {
                _logger.LogError(ea.Exception, "Recreating RabbitMQ consumer channel");

                _producerReceiveChannel?.Dispose();
                _producerReceiveChannel = StartConumer(queueName, receiveCallback);
            };

            // TODO: Add basic QOS here so that we only prefetch the number of items that we can simultaneously handle
            channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

            var consumer = new AsyncEventingBasicConsumer(channel);

            consumer.Received += receiveCallback;

            channel.BasicConsume(queue: queueName,
                                autoAck: false,
                                consumer: consumer);

            return channel;
        }

        public void Enqueue(TRequest data)
        {
            if (!_connection.IsConnected)
            {
                _connection.TryConnect();
            }

            if (_producerReceiveChannel == null)
            {
                lock (_syncRoot)
                {
                    if (_producerReceiveChannel == null) _producerReceiveChannel = StartConumer(ConsumerQueueName, OnQueueItemReceived);
                }
            }

            Publish(data, ConsumerQueueName);
        }

        public void NotifyCompleted(TRequest item, TaskStatus status, TResult? result)
        {
            if (!_connection.IsConnected)
            {
                _connection.TryConnect();
            }

            var eventArgs = new ItemCompletedEventArgs<TResult>(item.Id, status) { Result = result };

            Publish(eventArgs, NotifierQueueName);
        }

        private void Publish(object data, string queueName)
        {
            var body = JsonSerializer.SerializeToUtf8Bytes(data);
            var channel = GetChannelFromPool();            

            channel.ExchangeDeclare(exchange: RabbitMQConstants.ProducerConsumer.ExchangeName, type: "direct");           

            _retryPolicy.Execute(() =>
            {
                 channel.BasicPublish(exchange: RabbitMQConstants.ProducerConsumer.ExchangeName,
                                     routingKey: queueName,
                                     basicProperties: null,
                                     mandatory: true,
                                     body: body);
            });
        }

        private IModel GetChannelFromPool()
        {
            return _channelPool.GetOrAdd(Environment.CurrentManagedThreadId, key =>
            {
                var channel = _connection.CreateModel();
                channel.ConfirmSelect();

                channel.CallbackException += (_, _) =>
                {
                    channel?.Dispose();
                    if (_channelPool.TryRemove(Environment.CurrentManagedThreadId, out _))
                    {
                        channel = _channelPool.GetOrAdd(Environment.CurrentManagedThreadId, key => _connection.CreateModel());
                    }
                };

                return channel;
            });
        }
    }
}