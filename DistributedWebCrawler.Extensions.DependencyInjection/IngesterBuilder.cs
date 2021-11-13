using DistributedWebCrawler.Core;
using DistributedWebCrawler.Core.Components;
using DistributedWebCrawler.Core.Configuration;
using DistributedWebCrawler.Core.Extensions.DependencyInjection;
using DistributedWebCrawler.Core.Interfaces;
using DistributedWebCrawler.Core.Model;
using DistributedWebCrawler.Extensions.DependencyInjection.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace DistributedWebCrawler.Extensions.DependencyInjection
{
    public class IngesterBuilder : ComponentBuilder<IngestRequest, ParseRequest, IngesterSettings>, IIngesterBuilder
    {
        public IngesterBuilder(IServiceCollection services) : base(services)
        {
            services.AddSingleton<ICrawlerComponent, IngesterCrawlerComponent>();
        }
    }
}
