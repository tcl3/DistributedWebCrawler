using DistributedWebCrawler.Core.Configuration;
using DistributedWebCrawler.Core.Interfaces;
using DistributedWebCrawler.Core.Model;
using Microsoft.Extensions.Configuration;

namespace DistributedWebCrawler.Extensions.DependencyInjection.Interfaces
{
    public interface ISchedulerBuilder : IComponentBuilder<SchedulerRequest, IngestRequest, SchedulerSettings>
    {
        ISchedulerBuilder WithRobotsCache<TCache>(IConfiguration robotsTxtConfiguration) 
            where TCache : class, IRobotsCache;
        ISchedulerBuilder WithRobotsCache<TCache>(RobotsTxtSettings settings)
            where TCache : class, IRobotsCache;
    }
}
