using DistributedWebCrawler.Extensions.RabbitMQ.Interfaces;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace DistributedWebCrawler.Extensions.RabbitMQ
{
    internal static class PersistentConnectionExtensions
    {
        public static IModel StartConsumer(this IPersistentConnection connection, string? queueName, 
            AsyncEventHandler<BasicDeliverEventArgs> receiveCallback, ILogger? logger = null)
        {
            var channel = connection.CreateModel();

            channel.ExchangeDeclare(exchange: RabbitMQConstants.ProducerConsumer.ExchangeName, type: "direct");
            var x = channel.QueueDeclare(queue: queueName,
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

            channel.QueueBind(queue: queueName,
                exchange: RabbitMQConstants.ProducerConsumer.ExchangeName,
                routingKey: queueName);

            channel.CallbackException += (sender, ea) =>
            {
                logger?.LogError(ea.Exception, "Recreating RabbitMQ consumer channel");

                channel?.Dispose();
                channel = connection.StartConsumer(queueName, receiveCallback, logger);
            };

            channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

            var consumer = new AsyncEventingBasicConsumer(channel);

            consumer.Received += receiveCallback;

            channel.BasicConsume(queue: queueName,
                                autoAck: false,
                                consumer: consumer);

            return channel;
        }
    }
}
