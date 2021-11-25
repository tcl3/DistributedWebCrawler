using DistributedWebCrawler.Core.Configuration;
using DistributedWebCrawler.Core.Model;
using DistributedWebCrawler.Extensions.DependencyInjection.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace DistributedWebCrawler.Extensions.DependencyInjection
{
    internal class IngesterBuilder : ComponentBuilder<IngestRequest, bool, IngesterSettings>, IIngesterBuilder
    {
        public IngesterBuilder(IServiceCollection services) : base(services)
        {
        }
    }
}
