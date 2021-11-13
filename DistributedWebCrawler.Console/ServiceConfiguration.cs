using DistributedWebCrawler.Core.Interfaces;
using DistributedWebCrawler.Core.Exceptions;
using DistributedWebCrawler.Core.Seeding;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using System;
using System.Text;
using DistributedWebCrawler.Core.Extensions.DependencyInjection;
using DistributedWebCrawler.Extensions.DependencyInjection;

namespace DistributedWebCrawler.Core
{
    internal static class ServiceConfiguration
    {
        public static ICrawlerManager CreateCrawler(IConfiguration configuration)
        {
            return ConfigureServices(configuration).GetRequiredService<ICrawlerManager>();
        }

        public static IServiceProvider ConfigureServices(IConfiguration configuration)
        {
            var services = new ServiceCollection();

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
