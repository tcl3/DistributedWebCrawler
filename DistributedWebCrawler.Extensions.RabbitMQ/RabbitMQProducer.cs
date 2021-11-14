using DistributedWebCrawler.Core.Interfaces;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace DistributedWebCrawler.Extensions.RabbitMQ
{
    public class RabbitMQProducer<TData> : IProducer<TData> where TData : class
    {
        private readonly IConnection _connection;
        private readonly ILogger<RabbitMQProducer<TData>> _logger;
        private readonly RetryPolicy _retryPolicy;

        private const int RetryCount = 5;

        private static readonly string QueueName = typeof(TData).Name;

        public RabbitMQProducer(IConnection connection, ILogger<RabbitMQProducer<TData>> logger)
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
        }

        public void Enqueue(TData data)
        {
            var message = JsonSerializer.Serialize(data);
            var body = Encoding.UTF8.GetBytes(message);
            _retryPolicy.Execute(() =>
            {
                using var channel = _connection.CreateModel();
                var result = channel.QueueDeclare(queue: QueueName,
                                     durable: false,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                channel.BasicPublish(exchange: "",
                                     routingKey: QueueName,
                                     basicProperties: null,
                                     body: body);
            });
        }
    }
}