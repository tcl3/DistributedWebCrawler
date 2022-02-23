using DistributedWebCrawler.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Text;
using DistributedWebCrawler.Core.Extensions.DependencyInjection;
using Serilog;

namespace DistributedWebCrawler.Console
{
    internal static class ServiceConfiguration
    {
        public static IServiceProvider ConfigureServices(IConfiguration configuration, Serilog.ILogger logger)
        {
            var services = new ServiceCollection();

            services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.ClearProviders();
                loggingBuilder.AddSerilog(logger);
            });

            services.AddInMemoryCrawlerWithDefaultSettings(configuration);

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            return services.BuildServiceProvider();
        }
    }
}