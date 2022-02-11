using DistributedWebCrawler.Core.Enums;
using DistributedWebCrawler.Core.Interfaces;
using DistributedWebCrawler.Core.Models;
using DistributedWebCrawler.Extensions.RabbitMQ.Interfaces;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Runtime.Serialization;

namespace DistributedWebCrawler.Extensions.RabbitMQ
{
    public class RabbitMQComponentDecorator : ICrawlerComponent
    {
        private readonly ICrawlerComponent _inner;
        private readonly IPersistentConnection _connection;
        private readonly ISerializer _serializer;
        private IModel? _receiveChannel;


        public RabbitMQComponentDecorator(ICrawlerComponent inner, 
            IPersistentConnection connection,
            ISerializer serializer)
        {
            _inner = inner;
            _connection = connection;
            _serializer = serializer;
        }

        public CrawlerComponentStatus Status => _inner.Status;

        public ComponentInfo ComponentInfo => _inner.ComponentInfo;

        public Task PauseAsync()
        {
            return _inner.PauseAsync();
        }

        public Task ResumeAsync()
        {
            return _inner.ResumeAsync();
        }

        public Task StartAsync(CrawlerRunningState startState = CrawlerRunningState.Running, 
            CancellationToken cancellationToken = default)
        {
            if (_receiveChannel == null)
            {
                lock (_connection)
                {
                    if (_receiveChannel == null) _receiveChannel = StartConsumer();
                }
            }

            return _inner.StartAsync(startState);
        }

        public Task WaitUntilCompletedAsync()
        {
            return _inner.WaitUntilCompletedAsync();
        }

        private IModel StartConsumer()
        {
            if (!_connection.IsConnected)
            {
                _connection.TryConnect();
            }

            var channel = _connection.CreateModel();

            channel.ExchangeDeclare(exchange: RabbitMQConstants.CrawlerManager.ExchangeName, type: "fanout");
            
            var queueName = channel.QueueDeclare(durable: false, 
                exclusive: false, 
                autoDelete: true, 
                arguments: null).QueueName;
            
            channel.QueueBind(queue: queueName,
                exchange: RabbitMQConstants.CrawlerManager.ExchangeName,
                routingKey: "");

            channel.CallbackException += (sender, ea) =>
            {
                _receiveChannel?.Dispose();
                _receiveChannel = StartConsumer();
            };

            var consumer = new AsyncEventingBasicConsumer(channel);

            consumer.Received += OnCommandReceived;

            channel.BasicConsume(queue: queueName,
                                autoAck: true,
                                consumer: consumer);

            return channel;
        }

        private Task OnCommandReceived(object? model, BasicDeliverEventArgs ea)
        {
            var commandMessage = _serializer.Deserialize<RabbitMQCommandMessage>(ea.Body.Span);

            if (commandMessage == null)
            {
                throw new SerializationException($"Failed to deserialize RabbitMQ command message'");
            }

            if (!commandMessage.ComponentFilter.Matches(_inner))
            {
                return Task.CompletedTask;
            }

            switch (commandMessage.Command)
            {
                case Command.Pause:
                    _inner.PauseAsync();
                    break;
                case Command.Resume:
                    _inner.ResumeAsync();
                    break;
                default:
                    throw new InvalidOperationException($"Command type not implemented: '{commandMessage.Command}'");
            }

            return Task.CompletedTask;
        }
    }
}
