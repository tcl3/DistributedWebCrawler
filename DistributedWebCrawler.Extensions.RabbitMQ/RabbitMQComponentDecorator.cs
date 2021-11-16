using DistributedWebCrawler.Core.Enums;
using DistributedWebCrawler.Core.Interfaces;
using DistributedWebCrawler.Extensions.RabbitMQ.Interfaces;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace DistributedWebCrawler.Extensions.RabbitMQ
{
    public class RabbitMQComponentDecorator : ICrawlerComponent
    {
        private readonly ICrawlerComponent _inner;
        private readonly IPersistentConnection _connection;

        private IModel? _receiveChannel;

        private readonly string _queueName;

        public RabbitMQComponentDecorator(ICrawlerComponent inner, IPersistentConnection connection)
        {
            _inner = inner;
            _queueName = inner.GetType().Name + RabbitMQConstants.CrawlerManager.CommandQueueSuffix;
            _connection = connection;
        }

        public CrawlerComponentStatus Status => _inner.Status;

        public string Name => _inner.Name;

        public Task PauseAsync()
        {
            return _inner.PauseAsync();
        }

        public Task ResumeAsync()
        {
            return _inner.ResumeAsync();
        }

        public Task StartAsync(CrawlerStartState startState = CrawlerStartState.Running)
        {
            if (_receiveChannel == null)
            {
                lock (_connection)
                {
                    if (_receiveChannel == null) _receiveChannel = StartConumer();
                }
            }

            return _inner.StartAsync(startState);
        }

        public Task WaitUntilCompletedAsync()
        {
            return _inner.WaitUntilCompletedAsync();
        }

        private IModel StartConumer()
        {
            if (!_connection.IsConnected)
            {
                _connection.TryConnect();
            }
            var channel = _connection.CreateModel();

            channel.ExchangeDeclare(exchange: RabbitMQConstants.CrawlerManager.ExchangeName, type: "fanout");
            
            channel.QueueDeclare(queue: _queueName,
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);
            
            channel.QueueBind(queue: _queueName,
                exchange: RabbitMQConstants.CrawlerManager.ExchangeName,
                routingKey: "");

            channel.CallbackException += (sender, ea) =>
            {
                _receiveChannel?.Dispose();
                _receiveChannel = StartConumer();
            };

            var consumer = new EventingBasicConsumer(channel);

            consumer.Received += OnCommandReceived;

            channel.BasicConsume(queue: _queueName,
                                autoAck: true,
                                consumer: consumer);

            return channel;
        }

        private void OnCommandReceived(object? model, BasicDeliverEventArgs ea)
        {
            var messageString = Encoding.UTF8.GetString(ea.Body.Span);

            if (!Enum.TryParse<Command>(messageString, ignoreCase: true, out var commandType))
            {
                throw new InvalidOperationException($"Unknown command received: '{messageString}'");
            }

            switch (commandType)
            {
                case Command.Pause:
                    _inner.PauseAsync();
                    break;
                case Command.Resume:
                    _inner.ResumeAsync();
                    break;
                default:
                    throw new InvalidOperationException($"Command type not implemented: '{commandType}'");
            }
        }
    }
}
