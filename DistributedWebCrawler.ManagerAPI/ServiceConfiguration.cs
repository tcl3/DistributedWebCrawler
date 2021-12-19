using NLog.Extensions.Logging;
using System.Text;
using DistributedWebCrawler.Extensions.RabbitMQ;
using DistributedWebCrawler.ManagerAPI.Hubs;
using Microsoft.AspNetCore.ResponseCompression;

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

            services.AddResponseCompression(options =>
            {
                options.EnableForHttps = true;
            });

            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "wwwroot";
            });

            services.AddRabbitMQCrawlerManager(configuration);

            services.AddSignalR();
            services.AddSingleton<CrawlerHub>();
            services.AddSingleton<ComponentHubEventListener>();

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            return services.BuildServiceProvider();
        }
    }
}