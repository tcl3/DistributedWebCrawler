using DistributedWebCrawler.Core;
using DistributedWebCrawler.Core.Enums;
using DistributedWebCrawler.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.Threading.Tasks;

namespace DistributedWebCrawler.Console
{
    internal class Program
    {
        static async Task Main(string[] args)
        {            
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                .Build();

            var logger = new LoggerConfiguration()
                    .ReadFrom
                    .Configuration(configuration)
                    .CreateLogger();
            
            Log.Logger = logger;            

            try
            {
                var serviceProvider = ServiceConfiguration.ConfigureServices(configuration, logger);

                var crawlerManager = serviceProvider.GetRequiredService<ICrawlerManager>();

                await crawlerManager.StartAsync(CrawlerRunningState.Running);

                await crawlerManager.WaitUntilCompletedAsync();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Uncaught exception");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    }
}