using DistributedWebCrawler.Core.Configuration;
using DistributedWebCrawler.Core.Interfaces;
using DistributedWebCrawler.Core.Model;
using DistributedWebCrawler.Extensions.DependencyInjection.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace DistributedWebCrawler.Extensions.DependencyInjection
{
    public class SeederBuilder<TRequest> : ComponentBuilder<SeederSettings>, ISeederBuilder 
        where TRequest : RequestBase
    {
        public SeederBuilder(IServiceCollection services) : base(services)
        {
            services.AddSingleton<IProducer<TRequest>>(x => x.GetRequiredService<IProducerConsumer<TRequest>>());
        }

        ISeederBuilder ISeederBuilder.WithComponent<TComnponent>()
        {
            Services.AddSingleton<ISeederComponent, TComnponent>();
            return this;
        }
    }
}
