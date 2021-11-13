using DistributedWebCrawler.Core.Components;
using DistributedWebCrawler.Core.Configuration;
using DistributedWebCrawler.Core.Interfaces;
using DistributedWebCrawler.Core.Seeding;
using DistributedWebCrawler.Extensions.DependencyInjection;
using DistributedWebCrawler.Extensions.DependencyInjection.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;

namespace DistributedWebCrawler.Core.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {

        public static ISchedulerBuilder AddScheduler(this IServiceCollection services)
        {
            //services.AddSingleton<ISeederComponent, SchedulerQueueSeeder>();
            //services.AddSettings<SchedulerSettings>(configuration);

            //services.AddHttpClientWithSettings<RobotsClient, CrawlerClientSettings>(ConfigureDefaultClient)
            //    .ConfigurePrimaryHttpMessageHandler<CrawlerHttpClientHandler>();

            return new SchedulerBuilder(services);
        }

        public static IIngesterBuilder AddIngester(this IServiceCollection services)
        {
            return new IngesterBuilder(services);
        }

        public static IParserBuilder AddParser(this IServiceCollection services)
        {
            return new ParserBuilder(services);
        }

        public static ISeederBuilder AddSeeder(this IServiceCollection services)
        {
            return new SeederBuilder(services);
        }

        public static IServiceCollection AddQueue<TData, TQueue>(this IServiceCollection services)
            where TQueue : class, IProducerConsumer<TData>
        {
            services.AddSingleton<IProducerConsumer<TData>, TQueue>();
            services.AddSingleton<IProducer<TData>>(x => x.GetRequiredService<IProducerConsumer<TData>>());
            services.AddSingleton<IConsumer<TData>>(x => x.GetRequiredService<IProducerConsumer<TData>>());

            return services;
        }

        public static IServiceCollection AddPriorityQueue<TData, TPriority, TQueue>(this IServiceCollection services)
            where TQueue : class, IProducerConsumer<TData, TPriority>
        {
            services.AddSingleton<IProducerConsumer<TData, TPriority>, TQueue>();
            services.AddSingleton<IProducer<TData, TPriority>>(x => x.GetRequiredService<IProducerConsumer<TData, TPriority>>());
            services.AddSingleton<IConsumer<TData>>(x => x.GetRequiredService<IProducerConsumer<TData, TPriority>>());
            services.AddSingleton<IEqualityComparer<TData>>(x => x.GetService<IEqualityComparer<TData>>() ?? EqualityComparer<TData>.Default);
            services.AddSingleton<IComparer<TPriority>>(x => x.GetService<IComparer<TPriority>>() ?? Comparer<TPriority>.Default);

            return services;
        }

        public static IServiceCollection AddSettings<TSettings>(this IServiceCollection services,
            IConfiguration configuration)  where TSettings : class
        {
            services.AddOptions<TSettings>()
                .Bind(configuration)
                .ValidateDataAnnotations();

            return services.AddSingleton<TSettings>(x => x.GetRequiredService<IOptions<TSettings>>().Value);
        }

        public static IServiceCollection AddSettings<TSettings>(this IServiceCollection services, 
            IConfiguration configuration, string sectionName)
            where TSettings : class
        {
            var configurationSection = configuration.GetSection(sectionName);
            return services.AddSettings<TSettings>(configurationSection);
        }

        public static IHttpClientBuilder AddHttpClientWithSettings<TClient, TSettings>(this IServiceCollection services, Action<HttpClient, TSettings> clientConfigurationAction)
            where TClient : class
            where TSettings : class
        {
            return services.AddHttpClient<TClient>((serviceProvider, client) =>
            {
                var settings = serviceProvider.GetRequiredService<TSettings>();
                clientConfigurationAction(client, settings);
            });
        }
    }
}
