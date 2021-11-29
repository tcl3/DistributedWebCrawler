using DistributedWebCrawler.Core;
using DistributedWebCrawler.Core.Extensions.DependencyInjection;
using DistributedWebCrawler.Core.Interfaces;
using DistributedWebCrawler.Core.Seeding;
using DistributedWebCrawler.Extensions.RabbitMQ.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;

namespace DistributedWebCrawler.Extensions.RabbitMQ
{
    public static class ServiceCollectionExtensions
    {

        private static IServiceCollection AddRabbitMQConnection(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<IConnectionFactory>(_ =>
            {
                var connectionFactory = new ConnectionFactory
                {
                    AutomaticRecoveryEnabled = true,
                    DispatchConsumersAsync = true
                };

                var rabbitMQHostname = configuration.GetValue<string>("RABBITMQ_HOSTNAME");
                if (rabbitMQHostname != null)
                {
                    connectionFactory.HostName = rabbitMQHostname;
                }

                var rabbitMQUsername = configuration.GetValue<string>("RABBITMQ_USERNAME");
                if (rabbitMQUsername != null)
                {
                    connectionFactory.UserName = rabbitMQUsername;
                }

                var rabbitMQPassword = configuration.GetValue<string>("RABBITMQ_PASSWORD");
                if (rabbitMQPassword != null)
                {
                    connectionFactory.Password = rabbitMQPassword;
                }

                var rabbitMQPort = configuration.GetValue<int?>("RABBITMQ_PORT");
                if (rabbitMQPort.HasValue)
                {
                    connectionFactory.Port = rabbitMQPort.Value;
                }

                return connectionFactory;
            });

            services.AddSingleton<IPersistentConnection, RabbitMQPersistentConnection>();

            return services;
        }

        public static IServiceCollection AddRabbitMQProducerConsumer(this IServiceCollection services, IConfiguration configuration)
        {
            return services.AddRabbitMQConnection(configuration)
                .AddSingleton(typeof(IProducerConsumer<,>), typeof(RabbitMQProducerConsumer<,>))
                .AddSingleton(typeof(IEventDispatcher<,>), typeof(RabbitMQEventDispatcher<,>))
                .AddSingleton(typeof(IEventReceiver<,>), typeof(RabbitMQEventReceiver<,>))
                .AddSingleton<RabbitMQChannelPool>()
                .Decorate<ICrawlerComponent, RabbitMQComponentDecorator>();
        }

        public static IServiceCollection AddRabbitMQCrawlerManager(this IServiceCollection services, IConfiguration configuration)
        {
            return services.AddRabbitMQConnection(configuration)
                .AddInMemoryCrawlerManager()
                .Decorate<ICrawlerManager, RabbitMQCrawlerManagerDecorator>();
        }
    }
}