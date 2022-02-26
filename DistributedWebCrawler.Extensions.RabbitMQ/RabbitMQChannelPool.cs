using DistributedWebCrawler.Extensions.RabbitMQ.Interfaces;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using System.Net.Sockets;

namespace DistributedWebCrawler.Extensions.RabbitMQ
{
    public class RabbitMQChannelPool
    {
        private const int MaxPoolSize = 200;

        private readonly PooledChannel[] _channelPool;
        private int _channelsInUse;
        private readonly IPersistentConnection _connection;
        private readonly ILogger<RabbitMQChannelPool> _logger;
        private readonly RetryPolicy _retryPolicy;

        private readonly object channelPoolLock = new();

        public RabbitMQChannelPool(IPersistentConnection connection, ILogger<RabbitMQChannelPool> logger)
        {
            _channelPool = new PooledChannel[MaxPoolSize];
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

        public void PublishDirect(byte[] bytes, string exchangeName, string queueName)
        {
            Publish(bytes, exchangeName, exchangeType: "direct", routingKey: queueName);
        }

        public void PublishFanout(byte[] bytes, string exchangeName)
        {
            Publish(bytes, exchangeName, exchangeType: "fanout");
        }

        private void Publish(byte[] bytes, string exchangeName, string exchangeType, string routingKey = "")
        {
            if (!_connection.IsConnected)
            {
                _connection.TryConnect();
            }

            using var pooledChannel = GetPooledChannel();
            var channel = pooledChannel.Channel;

            channel.ExchangeDeclare(exchange: exchangeName, type: exchangeType);

            _retryPolicy.Execute(() =>
            {
                channel.BasicPublish(exchange: exchangeName,
                                    routingKey: routingKey,
                                    basicProperties: null,
                                    mandatory: true,
                                    body: bytes);
            });
        }

        public IPooledChannel GetPooledChannel()
        {
            lock(channelPoolLock)
            {
                var channelExists = TryGetChannelFromPool(out var index);

                PooledChannel pooledChannel;
                if (!channelExists)
                {
                    pooledChannel = new PooledChannel(CreateChannel());
                    _channelPool[index] = pooledChannel;
                }
                else
                {
                    pooledChannel = _channelPool[index];
                    pooledChannel.Reset();
                }

                return pooledChannel;
            }
        }

        private bool TryGetChannelFromPool(out int index)
        {            
            for (int i = 0; i < _channelsInUse; i++)
            {
                var currentChannel = _channelPool[i];
                
                if (currentChannel != null && currentChannel.IsDisposed)
                {
                    index = i;
                    return currentChannel != null;
                }
            }

            if (_channelsInUse == MaxPoolSize)
            {
                throw new InvalidOperationException($"RabbitMQ channel pool has reached capacity ({MaxPoolSize} channels)");
            }

            index = ++_channelsInUse;
            return false;
        }         
        
        private IModel CreateChannel()
        {
            var channel = _connection.CreateModel();
            channel.ConfirmSelect();

            channel.CallbackException += (_, _) =>
            {
                channel?.Dispose();
            };

            return channel;
        }

        private class PooledChannel : IPooledChannel
        {
            private readonly IModel _channel;

            internal bool IsDisposed { get; private set; }

            public PooledChannel(IModel channel)
            {
                _channel = channel;
            }

            public IModel Channel
            {
                get
                {
                    if (IsDisposed)
                    {
                        throw new ObjectDisposedException(nameof(PooledChannel));
                    }

                    return _channel;
                }
            }

            public void Dispose()
            {
                if (!IsDisposed)
                {
                    IsDisposed = true;
                }
            }

            internal void Reset()
            {
                IsDisposed = false;
            }
        }
    }
}