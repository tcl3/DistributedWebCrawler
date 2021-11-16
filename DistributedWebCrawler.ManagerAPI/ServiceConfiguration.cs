using DistributedWebCrawler.Core.Interfaces;
using DistributedWebCrawler.Core.Seeding;
using NLog.Extensions.Logging;
using System.Text;
using DistributedWebCrawler.Core.Extensions.DependencyInjection;
using DistributedWebCrawler.Core;
using DistributedWebCrawler.Extensions.DependencyInjection;
using DistributedWebCrawler.Extensions.RabbitMQ;
using DistributedWebCrawler.Core.Robots;

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

            services.AddScheduler()
                .WithRobotsCache<InMemoryRobotsCache>(crawlerConfiguration.GetSection("RobotsTxtSettings"))
                .WithSettings(crawlerConfiguration.GetSection("SchedulerSettings"))
                .WithClient<RobotsClient>(crawlerConfiguration.GetSection("CrawlerClientSettings"));

            services.AddIngester()
                .WithSettings(crawlerConfiguration.GetSection("IngesterSettings"))
                .WithClient<CrawlerClient>(crawlerConfiguration.GetSection("CrawlerClientSettings"));

            services.AddParser()
                .WithAngleSharpLinkParser()
                .WithSettings(crawlerConfiguration.GetSection("ParserSettings"));

            services.AddSingleton<ICrawlerManager, InMemoryCrawlerManager>();

            services.AddRabbitMQProducerConsumer(configuration);
            services.AddRabbitMQManager(configuration);
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            return services.BuildServiceProvider();
        }
    }
}