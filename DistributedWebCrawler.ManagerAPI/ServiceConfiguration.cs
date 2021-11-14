using DistributedWebCrawler.Core.Interfaces;
using DistributedWebCrawler.Core.Exceptions;
using DistributedWebCrawler.Core.Seeding;
using NLog.Extensions.Logging;
using System.Text;
using DistributedWebCrawler.Core.Extensions.DependencyInjection;
using DistributedWebCrawler.Core;
using DistributedWebCrawler.Extensions.DependencyInjection;
using DistributedWebCrawler.Extensions.RabbitMQ;
using DistributedWebCrawler.Core.Model;
using RabbitMQ.Client;

namespace DistributedWebCrawler.ManagerAPI
{
    internal static class ServiceConfiguration
    {
        public static IServiceProvider ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            var crawlerConfiguration = configuration.GetSection("CrawlerSettings");

            services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.ClearProviders();
                loggingBuilder.AddNLog();
            });

            services.AddSeeder()
                .WithComponent<SchedulerQueueSeeder>()
                .WithSettings(crawlerConfiguration.GetSection("SeederSettings"));

            services.AddSingleton<IConnectionFactory>(_ =>
            {
                var connectionFactory = new ConnectionFactory
                {
                    HostName = configuration.GetValue<string>("RABBITMQ_HOSTNAME"),
                    AutomaticRecoveryEnabled = true,
                };

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

            services.AddScheduler()
                .WithRobotsCache<InMemoryRobotsCache>(crawlerConfiguration.GetSection("RobotsTxtSettings"))
                .WithConsumer<RabbitMQConsumer<SchedulerRequest>>()
                .WithProducer<RabbitMQProducer<IngestRequest>>()
                .WithSettings(crawlerConfiguration.GetSection("SchedulerSettings"))
                .WithClient<RobotsClient>(crawlerConfiguration.GetSection("CrawlerClientSettings"));

            services.AddIngester()
                .WithConsumer<RabbitMQConsumer<IngestRequest>>()
                .WithProducer<RabbitMQProducer<ParseRequest>>()
                .WithSettings(crawlerConfiguration.GetSection("IngesterSettings"))
                .WithClient<CrawlerClient>(crawlerConfiguration.GetSection("CrawlerClientSettings"));

            services.AddParser()
                .WithAngleSharpLinkParser()
                .WithConsumer<RabbitMQConsumer<ParseRequest>>()
                .WithProducer<RabbitMQProducer<SchedulerRequest>>()
                .WithSettings(crawlerConfiguration.GetSection("ParserSettings"));

            services.AddSingleton<ICrawlerManager, CrawlerManager>();

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            return services.BuildServiceProvider();
        }
    }
}