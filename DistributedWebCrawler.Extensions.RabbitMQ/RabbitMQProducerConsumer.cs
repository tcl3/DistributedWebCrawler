using DistributedWebCrawler.Core.Interfaces;
using DistributedWebCrawler.Core.Models;
using DistributedWebCrawler.Extensions.RabbitMQ.Interfaces;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Collections.Concurrent;
using System.Runtime.Serialization;
using System.Text;

namespace DistributedWebCrawler.Extensions.RabbitMQ
{
    public class RabbitMQProducerConsumer<TRequest> : IProducerConsumer<TRequest>
        where TRequest : RequestBase
    {
        private readonly IPersistentConnection _connection;
        private readonly RabbitMQChannelPool _channelPool;
        private readonly ISerializer _serializer;
        private readonly ILogger<RabbitMQProducerConsumer<TRequest>> _logger;

        private static readonly string ConsumerQueueName = typeof(TRequest).Name;

        private IModel? _producerReceiveChannel;

        private readonly ConcurrentQueue<TaskCompletionSource<TRequest>> _taskQueue;
        private readonly SemaphoreSlim _messageSemaphore;

        private static readonly object _syncRoot = new();

        public int Count
        {
            get
            {
                if (!_connection.IsConnected)
                {
                    _connection.TryConnect();
                }

                using var pooledChannel = _channelPool.GetPooledChannel(RabbitMQConstants.ProducerConsumer.ExchangeName);

                var result = pooledChannel.Channel.QueueDeclare(queue: ConsumerQueueName,
                                     durable: false,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);
                
                return result.MessageCount <= int.MaxValue ? (int)result.MessageCount : int.MaxValue;
            }
        }

        public RabbitMQProducerConsumer(IPersistentConnection connection,
            RabbitMQChannelPool channelPool,
            ISerializer serializer,
            ILogger<RabbitMQProducerConsumer<TRequest>> logger)
        {
            _connection = connection;
            _channelPool = channelPool;
            _serializer = serializer;
            _logger = logger;

            _taskQueue = new();
            _messageSemaphore = new SemaphoreSlim(0);
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
                    if (_producerReceiveChannel == null)
                    {
                        _producerReceiveChannel = _connection.StartConsumerForComponent(ConsumerQueueName, OnQueueItemReceived, _logger);
                    } 
                }
            }

            var tcs = new TaskCompletionSource<TRequest>();
            _taskQueue.Enqueue(tcs);
            _messageSemaphore.Release();

            return tcs.Task;
        }
        private async Task OnQueueItemReceived(object? model, BasicDeliverEventArgs ea)
        {
            var queueItem = _serializer.Deserialize<TRequest>(ea.Body.Span);

            if (queueItem == null)
            {
                throw new SerializationException("Failed to deserialize queue item");
            }

            TaskCompletionSource<TRequest>? tcs;
            while (!_taskQueue.TryDequeue(out tcs))
            {
                await _messageSemaphore.WaitAsync().ConfigureAwait(false);
            }

            tcs.SetResult(queueItem);
            _producerReceiveChannel?.BasicAck(ea.DeliveryTag, multiple: false);
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
                    if (_producerReceiveChannel == null)
                    {
                        _producerReceiveChannel = _connection.StartConsumerForComponent(ConsumerQueueName, OnQueueItemReceived, _logger);
                    } 
                }
            }
            
            var bytes = _serializer.Serialize(data);

            _channelPool.PublishDirect(bytes, RabbitMQConstants.ProducerConsumer.ExchangeName, ConsumerQueueName);
        }
    }
}