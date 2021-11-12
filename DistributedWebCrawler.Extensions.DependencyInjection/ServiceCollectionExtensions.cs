using DistributedWebCrawler.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Net.Http;

namespace DistributedWebCrawler.Core.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
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
            IConfiguration configuration, string sectionName)
            where TSettings : class
        {
            services.AddOptions<TSettings>()
                .Bind(configuration.GetSection(sectionName))
                .ValidateDataAnnotations();

            return services.AddSingleton<TSettings>(x => x.GetRequiredService<IOptions<TSettings>>().Value);
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
