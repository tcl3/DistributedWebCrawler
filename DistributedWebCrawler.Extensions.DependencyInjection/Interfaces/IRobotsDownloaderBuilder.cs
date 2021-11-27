using DistributedWebCrawler.Core.Configuration;
using DistributedWebCrawler.Core.Interfaces;
using Microsoft.Extensions.Configuration;

namespace DistributedWebCrawler.Extensions.DependencyInjection.Interfaces
{
    public interface IRobotsDownloaderBuilder : IComponentBuilder<RobotsTxtSettings>
    {
    }
}
