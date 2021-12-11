using DistributedWebCrawler.Extensions.RabbitMQ.Interfaces;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace DistributedWebCrawler.Extensions.RabbitMQ
{
    internal static class PersistentConnectionExtensions
    {
        public static IModel StartConsumerForComponent(this IPersistentConnection connection, string queueName,
            AsyncEventHandler<BasicDeliverEventArgs> receiveCallback, ILogger? logger = null)
        {
            return connection.StartConsumer(RabbitMQConstants.ProducerConsumer.ExchangeName,
                exchangeType: "direct", receiveCallback, queueName, logger);
        }

        public static IModel StartConsumerForNotifier(this IPersistentConnection connection, string exchangeName,
            AsyncEventHandler<BasicDeliverEventArgs> receiveCallback, ILogger? logger = null)
        {
            return connection.StartConsumer(exchangeName,exchangeType: "fanout", receiveCallback, logger: logger);
        }

        private static IModel StartConsumer(this IPersistentConnection connection, string exchangeName, string exchangeType, 
            AsyncEventHandler<BasicDeliverEventArgs> receiveCallback, string queueName = "", ILogger? logger = null)
        {
            var channel = connection.CreateModel();

            channel.ExchangeDeclare(exchange: exchangeName, type: exchangeType);

            var queueDeclaration = channel.QueueDeclare(queue: queueName,
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: string.IsNullOrEmpty(queueName),
                                 arguments: null);

            channel.QueueBind(queue: queueName,
                    exchange: exchangeName,
                    routingKey: queueName);

            channel.CallbackException += (sender, ea) =>
            {
                logger?.LogError(ea.Exception, "Recreating RabbitMQ consumer channel");

                channel?.Dispose();
                channel = connection.StartConsumer(exchangeName, exchangeType, receiveCallback, queueName, logger);
            };

            channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

            var consumer = new AsyncEventingBasicConsumer(channel);

            consumer.Received += receiveCallback;

            channel.BasicConsume(queue: queueDeclaration.QueueName,
                                autoAck: false,
                                consumer: consumer);

            return channel;
        }
    }
}
