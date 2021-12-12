using DistributedWebCrawler.Core.Configuration;
using DistributedWebCrawler.Core.Interfaces;

namespace DistributedWebCrawler.Extensions.DependencyInjection.Interfaces
{
    public interface ISeederBuilder : IComponentBuilder<SeederSettings>
    {
        ISeederBuilder WithComponent<TComnponent>() where TComnponent : class, ISeederComponent;
    }
}