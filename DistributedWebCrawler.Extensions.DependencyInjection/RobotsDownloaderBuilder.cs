using DistributedWebCrawler.Core.Model;
using DistributedWebCrawler.Extensions.DependencyInjection.Configuration;
using DistributedWebCrawler.Extensions.DependencyInjection.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace DistributedWebCrawler.Extensions.DependencyInjection
{
    internal class RobotsDownloaderBuilder : ComponentBuilder<RobotsRequest, AnnotatedRobotsTxtSettings>, IRobotsDownloaderBuilder
    {
        public RobotsDownloaderBuilder(IServiceCollection services) : base(services)
        {
        }
    }
}
