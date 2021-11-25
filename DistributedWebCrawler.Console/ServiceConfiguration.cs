using DistributedWebCrawler.Core.Interfaces;
using DistributedWebCrawler.Core.Seeding;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using System;
using System.Text;
using DistributedWebCrawler.Core.Extensions.DependencyInjection;
using DistributedWebCrawler.Extensions.DependencyInjection;
using DistributedWebCrawler.Core.Robots;
using DistributedWebCrawler.Core.Model;
using DistributedWebCrawler.Core;

namespace DistributedWebCrawler.Console
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
                loggingBuilder.SetMinimumLevel(LogLevel.Trace);
                loggingBuilder.AddNLog();
            });

            services.AddInMemoryProducerConsumer();
            services.AddInMemoryKeyValueStore();

            services.AddCrawler(crawler => crawler
                .WithSeeder<SchedulerRequest>(seeder => seeder
                    .WithComponent<SchedulerQueueSeeder>()
                    .WithSettings(configuration.GetSection("SeederSettings")))
                .WithScheduler(scheduler => scheduler
                    .WithRobotsCache<InMemoryRobotsCache>(configuration.GetSection("RobotsTxtSettings"))
                    .WithSettings(configuration.GetSection("SchedulerSettings"))
                    .WithClient<RobotsClient>(configuration.GetSection("CrawlerClientSettings")))
                .WithIngester(ingester => ingester
                    .WithSettings(configuration.GetSection("IngesterSettings"))
                    .WithClient<CrawlerClient>(configuration.GetSection("CrawlerClientSettings")))
                .WithParser(parser => parser
                    .WithAngleSharpLinkParser()
                    .WithSettings(configuration.GetSection("ParserSettings"))));

            services.AddInMemoryCrawlerManager();

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            return services.BuildServiceProvider();
        }
    }
}