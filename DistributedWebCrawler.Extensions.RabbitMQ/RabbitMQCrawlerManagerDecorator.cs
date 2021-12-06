using DistributedWebCrawler.Core;
using DistributedWebCrawler.Core.Enums;
using DistributedWebCrawler.Core.Interfaces;
using DistributedWebCrawler.Core.Model;
using DistributedWebCrawler.Extensions.RabbitMQ.Interfaces;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using System.Net.Sockets;
using System.Text;

namespace DistributedWebCrawler.Extensions.RabbitMQ
{
    public class RabbitMQCrawlerManagerDecorator : ICrawlerManager
    {
        private readonly ICrawlerManager _inner;
        private readonly IPersistentConnection _connection;
        private readonly ILogger<RabbitMQCrawlerManagerDecorator> _logger;
        private readonly RetryPolicy _retryPolicy;

        public RabbitMQCrawlerManagerDecorator(ICrawlerManager inner, IPersistentConnection connection, ILogger<RabbitMQCrawlerManagerDecorator> logger)
        {
            _inner = inner;
            _connection = connection;
            _logger = logger;
            _retryPolicy = Policy.Handle<BrokerUnreachableException>()
               .Or<AlreadyClosedException>()
               .Or<SocketException>()
               .WaitAndRetry(RabbitMQConstants.CrawlerManager.RetryCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), (ex, time) =>
               {
                   _logger.LogWarning(ex, "Could not publish event {Timeout}s ({ExceptionMessage})", $"{time.TotalSeconds:n1}", ex.Message);
               });
        }

        public EventReceiverCollection Components => _inner.Components;

        public Task PauseAsync()
        {
            SendCommand(Command.Pause);
            return Task.CompletedTask;
        }

        public Task ResumeAsync()
        {
            SendCommand(Command.Resume);
            return Task.CompletedTask;
        }

        public Task StartAsync(CrawlerStartState startState = CrawlerStartState.Running)
        {
            return _inner.StartAsync(startState);
        }

        public Task WaitUntilCompletedAsync()
        {
            // TODO: Listen for completed signal here
            var tcs = new TaskCompletionSource();
            return Task.WhenAll(tcs.Task, _inner.WaitUntilCompletedAsync());
        }

        private void SendCommand(Command command)
        {
            var commandString = command.ToString();
            var commandBytes = Encoding.UTF8.GetBytes(commandString);

            if (!_connection.IsConnected)
            {
                _connection.TryConnect();
            }            
            
            using var channel = _connection.CreateModel();
            channel.ConfirmSelect();

            channel.ExchangeDeclare(exchange: RabbitMQConstants.CrawlerManager.ExchangeName, type: "fanout");
            
            _retryPolicy.Execute(() =>
            {
                channel.BasicPublish(exchange: RabbitMQConstants.CrawlerManager.ExchangeName,
                                     routingKey: "",
                                     basicProperties: null,
                                     body: commandBytes);
            });
        }
    }
}
