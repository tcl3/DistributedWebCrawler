using DistributedWebCrawler.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DistributedWebCrawler.Extensions.DependencyInjection.Interfaces
{
    public interface IComponentBuilder<TRequest, TResult, TSettings>
        where TRequest : class
        where TResult : class
        where TSettings : class
    {
        IServiceCollection Services { get; }
        IComponentBuilder<TRequest, TResult, TSettings> WithSettings(TSettings settings);
        IComponentBuilder<TRequest, TResult, TSettings> WithSettings(IConfiguration configuration);
        IComponentBuilder<TRequest, TResult, TSettings> WithConsumer<TConsumer>()
            where TConsumer : class, IConsumer<TRequest>;
        IComponentBuilder<TRequest, TResult, TSettings> WithProducer<TProducer>()
            where TProducer : class, IProducer<TResult>;
        IComponentBuilder<TRequest, TResult, TSettings> WithClient<TClient>(IConfiguration configuration)
            where TClient : class;
    }

    public interface IComponentBuilder<TSettings>
        where TSettings : class
    {
        IComponentBuilder<TSettings> WithSettings(TSettings settings);
        IComponentBuilder<TSettings> WithSettings(IConfiguration configuration);
    }
}