using DistributedWebCrawler.Core.Models;
using DistributedWebCrawler.Extensions.DependencyInjection.Configuration;
using DistributedWebCrawler.Extensions.DependencyInjection.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace DistributedWebCrawler.Extensions.DependencyInjection
{
    internal class IngesterBuilder : ComponentBuilder<IngestRequest, AnnotatedIngesterSettings>, IIngesterBuilder
    {
        public IngesterBuilder(IServiceCollection services) : base(services)
        {
        }
    }
}
