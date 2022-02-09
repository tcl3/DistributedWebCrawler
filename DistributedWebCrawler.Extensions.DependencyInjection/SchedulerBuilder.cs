using DistributedWebCrawler.Core.Models;
using DistributedWebCrawler.Extensions.DependencyInjection.Configuration;
using DistributedWebCrawler.Extensions.DependencyInjection.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace DistributedWebCrawler.Extensions.DependencyInjection
{
    internal class SchedulerBuilder : ComponentBuilder<SchedulerRequest, AnnotatedSchedulerSettings>, ISchedulerBuilder
    {
        public SchedulerBuilder(IServiceCollection services) : base(services)
        {
            
        }
    }
}
