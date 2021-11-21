using DistributedWebCrawler.Core.Configuration;
using DistributedWebCrawler.Core.Interfaces;
using DistributedWebCrawler.Core.Model;
using DistributedWebCrawler.Extensions.DependencyInjection.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace DistributedWebCrawler.Extensions.DependencyInjection
{
    public class SeederBuilder<TData> : ComponentBuilder<SeederSettings>, ISeederBuilder 
        where TData : RequestBase
    {
        public SeederBuilder(IServiceCollection services) : base(services)
        {
            services.AddSingleton<IProducer<TData>>(x => x.GetRequiredService<IProducerConsumer<TData>>());
        }

        ISeederBuilder ISeederBuilder.WithComponent<TComnponent>()
        {
            Services.AddSingleton<ISeederComponent, TComnponent>();
            return this;
        }
    }
}
