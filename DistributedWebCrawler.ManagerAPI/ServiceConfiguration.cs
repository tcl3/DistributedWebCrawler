using DistributedWebCrawler.Core.Interfaces;
using DistributedWebCrawler.Core.Seeding;
using NLog.Extensions.Logging;
using System.Text;
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

            services.AddRabbitMQCrawlerManager(configuration);
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            return services.BuildServiceProvider();
        }
    }
}