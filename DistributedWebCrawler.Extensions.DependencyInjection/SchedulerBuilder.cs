using DistributedWebCrawler.Core;
using DistributedWebCrawler.Core.Configuration;
using DistributedWebCrawler.Core.Extensions.DependencyInjection;
using DistributedWebCrawler.Core.Interfaces;
using DistributedWebCrawler.Core.Model;
using DistributedWebCrawler.Extensions.DependencyInjection.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DistributedWebCrawler.Extensions.DependencyInjection
{
    internal class SchedulerBuilder : ComponentBuilder<SchedulerRequest, bool, SchedulerSettings>, ISchedulerBuilder
    {
        public SchedulerBuilder(IServiceCollection services) : base(services)
        {
        }

        ISchedulerBuilder ISchedulerBuilder.WithRobotsCache<TCache>(IConfiguration robotsTxtConfiguration)
        {
            Services.AddSettings<RobotsTxtSettings>(robotsTxtConfiguration);
            Services.AddSingleton<IRobotsCache, TCache>();
            return this;
        }

        ISchedulerBuilder ISchedulerBuilder.WithRobotsCache<TCache>(RobotsTxtSettings settings)
        {
            Services.AddSingleton<RobotsTxtSettings>(settings);
            Services.AddSingleton<IRobotsCache, TCache>();
            return this;
        }
    }
}
