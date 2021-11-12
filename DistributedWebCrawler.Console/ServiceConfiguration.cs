using AngleSharp.Html.Parser;
using DistributedWebCrawler.Core.Components;
using DistributedWebCrawler.Core.Configuration;
using DistributedWebCrawler.Core.Extensions;
using DistributedWebCrawler.Core.Interfaces;
using DistributedWebCrawler.Core.LinkParser;
using DistributedWebCrawler.Core.Model;
using DistributedWebCrawler.Core.Queue;
using DistributedWebCrawler.Core.Exceptions;
using DistributedWebCrawler.Core.Seeding;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using DistributedWebCrawler.Core.Extensions.DependencyInjection;

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

            services.ConfigureDependencies(configuration);

            services.AddSingleton<CrawlerHttpClientHandler>();
            services.AddHttpClientWithSettings<CrawlerClient, CrawlerClientSettings>(ConfigureDefaultClient)
                .ConfigurePrimaryHttpMessageHandler<CrawlerHttpClientHandler>();
            services.AddHttpClientWithSettings<RobotsClient, CrawlerClientSettings>(ConfigureDefaultClient)
                .ConfigurePrimaryHttpMessageHandler<CrawlerHttpClientHandler>();

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            return services.BuildServiceProvider();
        }

        private static IServiceCollection ConfigureDependencies(this IServiceCollection services, IConfiguration configuration)
        {
            return services
                .AddSingleton<ISeederComponent, SchedulerQueueSeeder>()
                .AddSingleton<ISeeder, CompositeSeeder>()
                .AddSettings<CrawlerClientSettings>(configuration, "CrawlerClientSettings")
                .AddSettings<IngesterSettings>(configuration, "IngesterSettings")
                .AddSettings<SchedulerSettings>(configuration, "SchedulerSettings")
                .AddSettings<ParserSettings>(configuration, "ParserSettings")
                .AddSettings<RobotsTxtSettings>(configuration, "RobotsTxtSettings")
                .AddSettings<SeederSettings>(configuration, "SeederSettings")
                .AddSingleton<IRobotsCache, InMemoryRobotsCache>()
                .AddSingleton<ICrawlerComponentInterrogator, CrawlerComponentInterrogator>()
                .AddSingleton<ICrawlerManager, CrawlerManager>()
                .AddSingleton<IHtmlParser>(s => new HtmlParser()) // TODO: Move into provider?
                .AddSingleton<ILinkParser, AngleSharpLinkParser>()
                .AddSingleton<ICrawlerComponent, IngesterCrawlerComponent>()
                .AddSingleton<ICrawlerComponent, ParserCrawlerComponent>()
                .AddSingleton<ICrawlerComponent, SchedulerCrawlerComponent>()
                .AddSingleton<Lazy<IEnumerable<ICrawlerComponent>>>(x => new Lazy<IEnumerable<ICrawlerComponent>>(() => x.GetServices<ICrawlerComponent>()))
                .AddQueue<IngestRequest, InMemoryQueue<IngestRequest>>()
                .AddQueue<ParseRequest, InMemoryQueue<ParseRequest>>()
                .AddQueue<SchedulerRequest, InMemoryQueue<SchedulerRequest>>()
                ;
        }

        private static void ConfigureDefaultClient(HttpClient client, CrawlerClientSettings settings)
        {            
            if (settings.AcceptLanguage != null)
            {
                client.DefaultRequestHeaders.Add("Accept-Language", settings.AcceptLanguage);
            }
            client.DefaultRequestHeaders.Add("User-Agent", settings.UserAgentString);              
            client.DefaultRequestHeaders.Add("Accept", "*/*");
            client.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue { NoCache = true };
            client.Timeout = TimeSpan.FromSeconds(settings.TimeoutSeconds);
        }
    }
}
