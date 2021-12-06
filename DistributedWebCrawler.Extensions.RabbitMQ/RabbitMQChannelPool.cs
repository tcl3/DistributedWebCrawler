using DistributedWebCrawler.Core.Interfaces;
using DistributedWebCrawler.Extensions.RabbitMQ.Interfaces;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using System.Collections.Concurrent;
using System.Net.Sockets;

namespace DistributedWebCrawler.Extensions.RabbitMQ
{
    public class RabbitMQChannelPool
    {
        private readonly ConcurrentDictionary<int, IModel> _channelPool;
        private readonly IPersistentConnection _connection;
        private readonly ILogger<RabbitMQChannelPool> _logger;
        private readonly RetryPolicy _retryPolicy;

        public RabbitMQChannelPool(IPersistentConnection connection, ILogger<RabbitMQChannelPool> logger)
        {
            _channelPool = new();
            _connection = connection;
            _logger = logger;
            _retryPolicy = Policy.Handle<BrokerUnreachableException>()
                .Or<AlreadyClosedException>()
                .Or<SocketException>()
                .WaitAndRetry(RabbitMQConstants.ProducerConsumer.RetryCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), (ex, time) =>
                {
                    _logger.LogWarning(ex, "Could not publish event {Timeout}s ({ExceptionMessage})", $"{time.TotalSeconds:n1}", ex.Message);
                });
        }

        public void Publish(byte[] bytes, string exchangeName, string queueName)
        {
            if (!_connection.IsConnected)
            {
                _connection.TryConnect();
            }

            var channel = GetChannel();

            channel.ExchangeDeclare(exchange: exchangeName, type: "direct");

            _retryPolicy.Execute(() =>
            {
                channel.BasicPublish(exchange: RabbitMQConstants.ProducerConsumer.ExchangeName,
                                    routingKey: queueName,
                                    basicProperties: null,
                                    mandatory: true,
                                    body: bytes);
            });
        }

        public IModel GetChannel()
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