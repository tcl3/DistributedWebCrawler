using DistributedWebCrawler.Core;
using DistributedWebCrawler.Core.Configuration;
using DistributedWebCrawler.Core.Extensions.DependencyInjection;
using DistributedWebCrawler.Core.Interfaces;
using DistributedWebCrawler.Extensions.DependencyInjection.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
    }

    public abstract class ComponentBuilder<TRequest, TResult, TSettings> 
        : IComponentBuilder<TRequest, TResult, TSettings>
        where TRequest : class
        where TResult : class
        where TSettings : class
    {
        public ComponentBuilder(IServiceCollection services)
        {
            Services = services;
        }

        public IServiceCollection Services { get; }

        public IComponentBuilder<TRequest, TResult, TSettings> WithConsumer<TConsumer>() 
            where TConsumer : class, IConsumer<TRequest>
        {
            Services.AddSingleton<TConsumer>();
            return this;
        }

        public IComponentBuilder<TRequest, TResult, TSettings> WithProducer<TProducer>() 
            where TProducer : class, IProducer<TResult>
        {
            Services.AddSingleton<TProducer>();
            return this;
        }

        public IComponentBuilder<TRequest, TResult, TSettings> WithSettings(TSettings settings)
        {
            Services.AddSingleton<TSettings>(settings);
            return this;
        }

        public IComponentBuilder<TRequest, TResult, TSettings> WithSettings(IConfiguration configuration)
        {
            Services.AddSettings<TSettings>(configuration);
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
            client.Timeout = TimeSpan.FromSeconds(settings.TimeoutSeconds);
        }

        IComponentBuilder<TRequest, TResult, TSettings> IComponentBuilder<TRequest, TResult, TSettings>.WithClient<TClient>(IConfiguration configuration)
        {
            Services.AddSettings<CrawlerClientSettings>(configuration);
            Services.AddSingleton<CrawlerHttpClientHandler>();
            Services.AddHttpClientWithSettings<TClient, CrawlerClientSettings>(ConfigureClient)
                .ConfigurePrimaryHttpMessageHandler<CrawlerHttpClientHandler>();

            return this;
        }
    }
}
