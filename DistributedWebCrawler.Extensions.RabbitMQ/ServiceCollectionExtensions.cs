using DistributedWebCrawler.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;

namespace DistributedWebCrawler.Extensions.RabbitMQ
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRabbitMQProducerConsumer(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton(typeof(IProducerConsumer<>), typeof(RabbitMQProducerConsumer<>));

            services.AddSingleton<IConnectionFactory>(_ =>
            {
                var connectionFactory = new ConnectionFactory
                {
                    AutomaticRecoveryEnabled = true,
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

            services.AddSingleton<IConnection>(s =>
            {
                var connectionFactory = s.GetRequiredService<IConnectionFactory>();
                return connectionFactory.CreateConnection();
            });

            return services;
        }
    }
}