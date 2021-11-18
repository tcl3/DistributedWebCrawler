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

        private async Task OnMessageReceived(object? model, BasicDeliverEventArgs ea)
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
                await _messageSemaphore.WaitAsync().ConfigureAwait(false);
            }

            tcs.SetResult(queueItem);
            _receiveChannel?.BasicAck(ea.DeliveryTag, multiple: false);
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
                _logger.LogError(ea.Exception, "Recreating RabbitMQ consumer channel");

                _receiveChannel?.Dispose();
                _receiveChannel = StartConumer();
            };

            // TODO: Add basic QOS here so that we only prefetch the number of items that we can simultaneously handle
            //channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

            var consumer = new AsyncEventingBasicConsumer(channel);

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

            channel.ConfirmSelect();

            var outstandingConfirms = new ConcurrentDictionary<ulong, bool>();

            void CleanOutstandingConfirms(ulong sequenceNumber, bool multiple)
            {
                if (multiple)
                {
                    var confirmed = outstandingConfirms.Where(k => k.Key <= sequenceNumber);
                    foreach (var entry in confirmed)
                        outstandingConfirms.TryRemove(entry.Key, out _);
                }
                else
                    outstandingConfirms.TryRemove(sequenceNumber, out _);
            }

            channel.ExchangeDeclare(exchange: RabbitMQConstants.ProducerConsumer.ExchangeName, type: "direct");
            
            channel.BasicAcks += (sender, ea) => CleanOutstandingConfirms(ea.DeliveryTag, ea.Multiple);
            channel.BasicNacks += (sender, ea) =>
            {
                outstandingConfirms.TryGetValue(ea.DeliveryTag, out bool body);
                _logger.LogInformation($"Message has been nack-ed. Sequence number: {ea.DeliveryTag}, multiple: {ea.Multiple}");
                CleanOutstandingConfirms(ea.DeliveryTag, ea.Multiple);
            };

            _retryPolicy.Execute(() =>
            {
                outstandingConfirms.TryAdd(channel.NextPublishSeqNo, true);
                channel.BasicPublish(exchange: RabbitMQConstants.ProducerConsumer.ExchangeName,
                                     routingKey: QueueName,
                                     basicProperties: null,
                                     mandatory: true,                                     
                                     body: body);
            });
        }

    }
}
