using DistributedWebCrawler.Core.Interfaces;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DistributedWebCrawler.Extensions.RabbitMQ
{
    public class RabbitMQProducerConsumer<TData> : IProducerConsumer<TData>
    {
        private readonly IConnection _connection;
        private readonly ILogger<RabbitMQProducerConsumer<TData>> _logger;
        private readonly RetryPolicy _retryPolicy;

        private const int RetryCount = 5;

        private static readonly string QueueName = typeof(TData).Name;

        private IModel? _receiveChannel;

        private readonly ConcurrentQueue<TaskCompletionSource<TData>> _taskQueue;


        public RabbitMQProducerConsumer(IConnection connection, ILogger<RabbitMQProducerConsumer<TData>> logger)
        {
            _connection = connection;
            _logger = logger;

            _retryPolicy = Policy.Handle<BrokerUnreachableException>()
               .Or<AlreadyClosedException>()
               .Or<SocketException>()
               .WaitAndRetry(RetryCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), (ex, time) =>
               {
                   _logger.LogWarning(ex, "Could not publish event {Timeout}s ({ExceptionMessage})", $"{time.TotalSeconds:n1}", ex.Message);
               });

            _taskQueue = new();
        }

        public Task<TData> DequeueAsync()
        {
            if (_receiveChannel == null)
            {
                lock(_connection)
                {
                    if (_receiveChannel == null) _receiveChannel = StartConumer();
                }
            }

            var tcs = new TaskCompletionSource<TData>();
            _taskQueue.Enqueue(tcs);

            return tcs.Task;
        }

        private void OnMessageReceived(object? model, BasicDeliverEventArgs ea)
        {
            try
            {
                var body = ea.Body.ToArray();
                var messageJson = Encoding.UTF8.GetString(body);

                _logger.LogInformation($"Message received: '{messageJson}'");

                var queueItem = JsonSerializer.Deserialize<TData>(messageJson);

                if (queueItem != null && _taskQueue.TryDequeue(out var tcs))
                {
                    tcs.SetResult(queueItem);
                }
            }
            finally
            {
                _receiveChannel.BasicAck(ea.DeliveryTag, multiple: false);
            }
        }

        private IModel StartConumer()
        {
            var channel = _connection.CreateModel();

            channel.QueueDeclare(queue: QueueName,
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

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
            var body = JsonSerializer.SerializeToUtf8Bytes(data);
            using var channel = _connection.CreateModel();
            channel.QueueDeclare(queue: QueueName,
                                     durable: false,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

            _retryPolicy.Execute(() =>
            {
                channel.BasicPublish(exchange: "",
                                     routingKey: QueueName,
                                     basicProperties: null,
                                     body: body);
            });
        }
    }
}
