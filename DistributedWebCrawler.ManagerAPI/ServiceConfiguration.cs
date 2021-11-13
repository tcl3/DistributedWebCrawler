using DistributedWebCrawler.Core.Interfaces;
using DistributedWebCrawler.Core.Exceptions;
using DistributedWebCrawler.Core.Seeding;
using NLog.Extensions.Logging;
using System.Text;
using DistributedWebCrawler.Core.Extensions.DependencyInjection;
using DistributedWebCrawler.Core;
using DistributedWebCrawler.Extensions.DependencyInjection;

namespace DistributedWebCrawler.ManagerAPI
{
    internal static class ServiceConfiguration
    {
        public static IServiceProvider ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.ClearProviders();
                loggingBuilder.AddNLog();
            });

            services.AddSeeder()
                .WithComponent<SchedulerQueueSeeder>()
                .WithSettings(configuration.GetSection("SeederSettings"));

            services.AddScheduler()
                .WithRobotsCache<InMemoryRobotsCache>(configuration.GetSection("RobotsTxtSettings"))
                .WithInMemoryProducerConsumer()
                .WithSettings(configuration.GetSection("SchedulerSettings"))
                .WithClient<RobotsClient>(configuration.GetSection("CrawlerClientSettings"));

            services.AddIngester()
                .WithInMemoryProducerConsumer()
                .WithSettings(configuration.GetSection("IngesterSettings"))
                .WithClient<CrawlerClient>(configuration.GetSection("CrawlerClientSettings"));

            services.AddParser()
                .WithAngleSharpLinkParser()
                .WithInMemoryProducerConsumer()
                .WithSettings(configuration.GetSection("ParserSettings"));

            services.AddSingleton<ICrawlerManager, CrawlerManager>();

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            return services.BuildServiceProvider();
        }
    }
}
