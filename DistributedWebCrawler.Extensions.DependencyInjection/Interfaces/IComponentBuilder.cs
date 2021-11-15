using DistributedWebCrawler.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DistributedWebCrawler.Extensions.DependencyInjection.Interfaces
{
    public interface IComponentBuilder<TSettings>
        where TSettings : class
    {
        IComponentBuilder<TSettings> WithSettings(TSettings settings);
        IComponentBuilder<TSettings> WithSettings(IConfiguration configuration);
        IComponentBuilder<TSettings> WithClient<TClient>(IConfiguration configuration) 
            where TClient : class;
        IServiceCollection Services { get; }
    }
}