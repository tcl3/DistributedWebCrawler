using DistributedWebCrawler.Core.Configuration;
using DistributedWebCrawler.Core.Model;

namespace DistributedWebCrawler.Extensions.DependencyInjection.Interfaces
{
    public interface IIngesterBuilder : IComponentBuilder<IngestRequest, ParseRequest, IngesterSettings>
    {
    }
}
