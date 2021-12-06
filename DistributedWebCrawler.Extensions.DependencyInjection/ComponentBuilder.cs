using DistributedWebCrawler.Core;
using DistributedWebCrawler.Core.Configuration;
using DistributedWebCrawler.Core.Extensions.DependencyInjection;
using DistributedWebCrawler.Core.Interfaces;
using DistributedWebCrawler.Core.Model;
using DistributedWebCrawler.Extensions.DependencyInjection.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Headers;

namespace DistributedWebCrawler.Extensions.DependencyInjection
{

    public abstract class ComponentBuilder<TSettings> : IComponentBuilder<TSettings> 
        where TSettings : class
    {
        public ComponentBuilder(IServiceCollection services)
        {
            Services = services;
        }

        public IServiceCollection Services { get; }

        public IComponentBuilder<TSettings> WithSettings(TSettings settings)
        {
            Services.AddSingleton<TSettings>(settings);
            return this;
        }

        public IComponentBuilder<TSettings> WithSettings(IConfiguration configuration)
        {
            Services.AddSettings<TSettings>(configuration);
            return this;
        }

        IComponentBuilder<TSettings> IComponentBuilder<TSettings>.WithClient<TClient>(IConfiguration configuration, bool allowAutoRedirect)
        {
            Services.AddSettings<CrawlerClientSettings>(configuration);
            Services.AddHttpClientWithSettings<TClient, CrawlerClientSettings>(ConfigureClient)
                .ConfigurePrimaryHttpMessageHandler(serviceProvider =>
                {
                    var clientSettings = serviceProvider.GetRequiredService<CrawlerClientSettings>();
                    return new SocketsHttpHandler
                    {
                        AllowAutoRedirect = allowAutoRedirect,
                        AutomaticDecompression = DecompressionMethods.All,
                        ConnectTimeout = TimeSpan.FromSeconds(clientSettings.ConnectTimeoutSeconds),
                        ResponseDrainTimeout = TimeSpan.FromSeconds(clientSettings.ResponseDrainTimeoutSeconds),
                        MaxConnectionsPerServer = clientSettings.MaxConnectionsPerServer,
                    };
                });

            return this;
        }

        protected static void ConfigureClient(HttpClient client, CrawlerClientSettings settings)
        {
            if (settings.AcceptLanguage != null)
            {
                client.DefaultRequestHeaders.Add("Accept-Language", settings.AcceptLanguage);
            }
            client.DefaultRequestHeaders.Add("User-Agent", settings.UserAgentString);
            client.DefaultRequestHeaders.Add("Accept", "*/*");
            client.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue { NoCache = true };
            client.Timeout = TimeSpan.FromSeconds(settings.RequestTimeoutSeconds);
        }
    }

    public abstract class ComponentBuilder<TRequest, TSettings> : ComponentBuilder<TSettings>
        where TRequest : RequestBase
        where TSettings : class
    {
        public ComponentBuilder(IServiceCollection services) : base(services)
        {
            services.AddSingleton<IProducer<TRequest>>(x => x.GetRequiredService<IProducerConsumer<TRequest>>());
            services.AddSingleton<IConsumer<TRequest>>(x => x.GetRequiredService<IProducerConsumer<TRequest>>());
        }
    }
}
