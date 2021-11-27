using DistributedWebCrawler.Core.Configuration;
using DistributedWebCrawler.Core.Extensions.DependencyInjection;
using DistributedWebCrawler.Core.Interfaces;
using DistributedWebCrawler.Core.Model;
using DistributedWebCrawler.Core.Robots;
using DistributedWebCrawler.Extensions.DependencyInjection.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DistributedWebCrawler.Extensions.DependencyInjection
{
    internal class RobotsDownloaderBuilder : ComponentBuilder<RobotsRequest, bool, RobotsTxtSettings>, IRobotsDownloaderBuilder
    {
        public RobotsDownloaderBuilder(IServiceCollection services) : base(services)
        {
        }
    }
}
