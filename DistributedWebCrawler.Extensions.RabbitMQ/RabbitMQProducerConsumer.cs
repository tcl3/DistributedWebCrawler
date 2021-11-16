using DistributedWebCrawler.Core.Interfaces;
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
    public class RabbitMQProducerConsumer<TData> : IProducerConsumer<TData>
    {
        private readonly IPersistentConnection _connection;
        private readonly ILogger<RabbitMQProducerConsumer<TData>> _logger;
        private readonly RetryPolicy _retryPolicy;

        private static readonly string QueueName = typeof(TData).Name;

        private IModel? _receiveChannel;

        private readonly ConcurrentQueue<TaskCompletionSource<TData>> _taskQueue;
        private readonly SemaphoreSlim _messageSemaphore;

        private static readonly object _syncRoot = new object();

        public RabbitMQProducerConsumer(IPersistentConnection connection, ILogger<RabbitMQProducerConsumer<TData>> logger)
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
        }

        public Task<TData> DequeueAsync()
        {
            if (_receiveChannel == null)
            {
                lock(_syncRoot)
                {
                    if (_receiveChannel == null) _receiveChannel = StartConumer();
                }
            }

            var tcs = new TaskCompletionSource<TData>();
            _taskQueue.Enqueue(tcs);
            _messageSemaphore.Release();

            return tcs.Task;
        }

        private void OnMessageReceived(object? model, BasicDeliverEventArgs ea)
        {
            try
            {
                _logger.LogDebug($"Message received: '{Encoding.UTF8.GetString(ea.Body.Span)}'");

                var queueItem = JsonSerializer.Deserialize<TData>(ea.Body.Span);

                if (queueItem == null)
                {
                    throw new JsonException("Failed to deserialize queue item");
                }

                TaskCompletionSource<TData> tcs;
                while (!_taskQueue.TryDequeue(out tcs)) 
                {
                    _messageSemaphore.Wait();
                }

                tcs.SetResult(queueItem);
            }
            finally
            {
                _receiveChannel?.BasicAck(ea.DeliveryTag, multiple: false);
            }
        }

        private IModel StartConumer()
        {
            if (!_connection.IsConnected)
            {
                _connection.TryConnect();
            }

            var channel = _connection.CreateModel();

            channel.ExchangeDeclare(exchange: RabbitMQConstants.ProducerConsumer.ExchangeName, type: "direct");
            channel.QueueDeclare(queue: QueueName,
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);
            
            channel.QueueBind(queue: QueueName, 
                exchange: RabbitMQConstants.ProducerConsumer.ExchangeName, 
                routingKey: QueueName);

            channel.CallbackException += (sender, ea) =>
            {
                _logger.LogWarning(ea.Exception, "Recreating RabbitMQ consumer channel");

                _receiveChannel?.Dispose();
                _receiveChannel = StartConumer();
            };

            // TODO: Add basic QOS here so that we only prefetch the number of items that we can simultaneously handle
            //channel.BasicQos(0, 50, false);

            var consumer = new EventingBasicConsumer(channel);

            consumer.Received += OnMessageReceived;

            channel.BasicConsume(queue: QueueName,
                                autoAck: false,
                                consumer: consumer);

            return channel;
        }

        public void Enqueue(TData data)
        {
            if (!_connection.IsConnected)
            {
                _connection.TryConnect();
            }

            if (_receiveChannel == null)
            {
                lock (_syncRoot)
                {
                    if (_receiveChannel == null) _receiveChannel = StartConumer();
                }
            }

            var body = JsonSerializer.SerializeToUtf8Bytes(data);
            using var channel = _connection.CreateModel();
            
            _retryPolicy.Execute(() =>
            {
                channel.BasicPublish(exchange: RabbitMQConstants.ProducerConsumer.ExchangeName,
                                     routingKey: QueueName,
                                     basicProperties: null,
                                     body: body);
            });
        }
    }
}
