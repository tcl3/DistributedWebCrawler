using DistributedWebCrawler.Core.Interfaces;
using DistributedWebCrawler.Core.Seeding;
using NLog.Extensions.Logging;
using System.Text;
using DistributedWebCrawler.Core;
using DistributedWebCrawler.Extensions.RabbitMQ;

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

            services.AddSingleton<ISeeder, CompositeSeeder>();
            services.AddSingleton<ICrawlerManager, InMemoryCrawlerManager>();

            services.AddRabbitMQManager(configuration);
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            return services.BuildServiceProvider();
        }
    }
}