using DistributedWebCrawler.Core.Interfaces;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;

namespace DistributedWebCrawler.Extensions.RabbitMQ
{
    public class RabbitMQConsumer<TData> : IConsumer<TData> where TData : class
    {
        private readonly IConnection _connection;
        private IModel? _channel;
        private readonly ILogger<RabbitMQConsumer<TData>> _logger;

        private static readonly string QueueName = typeof(TData).Name;

        private readonly ConcurrentQueue<TaskCompletionSource<TData>> _taskQueue;

        public RabbitMQConsumer(IConnection connection, ILogger<RabbitMQConsumer<TData>> logger)
        {
            _connection = connection;
            _logger = logger;

            _taskQueue = new();
        }

        public Task<TData> DequeueAsync()
        {
            if (_channel == null)
            {
                lock (_connection)
                {
                    if (_channel == null) StartConumer();
                }
            }

            var tcs = new TaskCompletionSource<TData>();
            _taskQueue.Enqueue(tcs);
            
            return tcs.Task;
        }

        private void OnMessageReceived(object? model, BasicDeliverEventArgs ea)
        {
            var body = ea.Body.ToArray();
            var messageJson = Encoding.UTF8.GetString(body);

            _logger.LogInformation($"Message received: '{messageJson}'");
            
            if (_channel != null)
            {
                _channel.BasicAck(ea.DeliveryTag, multiple: false);
            }

            var queueItem = JsonSerializer.Deserialize<TData>(messageJson);

            if (queueItem != null && _taskQueue.TryDequeue(out var tcs)) 
            {
                tcs.SetResult(queueItem);
            }
        }

        private void StartConumer()
        {
            var channel = _connection.CreateModel();

            channel.QueueDeclare(queue: QueueName,
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

            // TODO: Add basic QOS here so that we only prefetch the number of items that we can simultaneously handle
            //channel.BasicQos(0, MaxConcurrentItems, false);

            var consumer = new EventingBasicConsumer(channel);

            consumer.Received += OnMessageReceived;

            channel.BasicConsume(queue: QueueName,
                                autoAck: false,
                                consumer: consumer);

            _channel = channel;
        }
    }
}
