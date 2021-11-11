﻿using DistributedWebCrawler.Core;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Threading.Tasks;

namespace DistributedWebCrawler.Console
{
    static class Program
    {
        static Program() 
        {
            ServicePointManager.DefaultConnectionLimit = 1000;
        }

        static async Task Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                           .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                           .Build();

            var crawlerManager = ServiceConfiguration.CreateCrawler(configuration);

           await crawlerManager.StartAsync();

            // For testing pause / resume functionality
            //await Task.Delay(5000);
            //await crawlerManager.PauseAsync();
            //await Task.Delay(40000);
            //await crawlerManager.ResumeAsync();

            await crawlerManager.WaitUntilCompletedAsync();
        }        
    }
}