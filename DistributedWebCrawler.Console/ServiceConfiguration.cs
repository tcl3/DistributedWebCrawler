﻿using DistributedWebCrawler.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using System;
using System.Text;
using DistributedWebCrawler.Extensions.RabbitMQ;
using DistributedWebCrawler.Core.Extensions.DependencyInjection;

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

            services.AddInMemoryCrawlerWithDefaultSettings(configuration);

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            return services.BuildServiceProvider();
        }
    }
}