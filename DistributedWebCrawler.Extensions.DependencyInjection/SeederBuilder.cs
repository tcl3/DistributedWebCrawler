using DistributedWebCrawler.Core.Configuration;
using DistributedWebCrawler.Core.Interfaces;
using DistributedWebCrawler.Core.Seeding;
using DistributedWebCrawler.Extensions.DependencyInjection.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace DistributedWebCrawler.Extensions.DependencyInjection
{
    public class SeederBuilder : ComponentBuilder<SeederSettings>, ISeederBuilder
    {
        public SeederBuilder(IServiceCollection services) : base(services)
        {
            services.AddSingleton<ISeeder, CompositeSeeder>();
        }

        ISeederBuilder ISeederBuilder.WithComponent<TComnponent>()
        {
            Services.AddSingleton<ISeederComponent, TComnponent>();
            return this;
        }
    }
}
