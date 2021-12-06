using DistributedWebCrawler.Core.Configuration;
using DistributedWebCrawler.Core.Interfaces;
using DistributedWebCrawler.Core.Model;
using DistributedWebCrawler.Core.Robots;
using DistributedWebCrawler.Extensions.DependencyInjection.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace DistributedWebCrawler.Extensions.DependencyInjection
{
    internal class SchedulerBuilder : ComponentBuilder<SchedulerRequest, SchedulerSettings>, ISchedulerBuilder
    {
        public SchedulerBuilder(IServiceCollection services) : base(services)
        {
            
        }
    }
}
