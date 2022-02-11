using DistributedWebCrawler.Core;
using DistributedWebCrawler.Core.Enums;
using DistributedWebCrawler.Core.Interfaces;
using DistributedWebCrawler.Core.Models;
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
        private readonly ISerializer _serializer;
        private readonly ILogger<RabbitMQCrawlerManagerDecorator> _logger;
        private readonly RetryPolicy _retryPolicy;

        public RabbitMQCrawlerManagerDecorator(ICrawlerManager inner, 
            IPersistentConnection connection, 
            ISerializer serializer,
            ILogger<RabbitMQCrawlerManagerDecorator> logger)
        {
            _inner = inner;
            _connection = connection;
            _serializer = serializer;
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
            return PauseAsync(ComponentFilter.MatchAll);
        }

        public Task PauseAsync(ComponentFilter componentFilter)
        {
            SendCommand(Command.Pause, componentFilter);
            return Task.CompletedTask;
        }

        public Task ResumeAsync()
        {
            return ResumeAsync(ComponentFilter.MatchAll);
        }

        public Task ResumeAsync(ComponentFilter componentFilter)
        {
            SendCommand(Command.Resume, componentFilter);
            return Task.CompletedTask;
        }

            public Task StartAsync(CrawlerRunningState startState = CrawlerRunningState.Running)
        {
            return _inner.StartAsync(startState);
        }

        public Task WaitUntilCompletedAsync()
        {
            return WaitUntilCompletedAsync(ComponentFilter.MatchAll);
        }

        public Task WaitUntilCompletedAsync(ComponentFilter componentFilter)
        {
            // TODO: Listen for completed signal here
            var tcs = new TaskCompletionSource();
            return Task.WhenAll(tcs.Task, _inner.WaitUntilCompletedAsync(componentFilter));
        }

        private void SendCommand(Command command, ComponentFilter componentFilter)
        {
            var commandMessage = new RabbitMQCommandMessage(command, componentFilter);
            
            var messageBytes = _serializer.Serialize(commandMessage);

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
                                     body: messageBytes);
            });
        }
    }
}
