using DistributedWebCrawler.Core;
using DistributedWebCrawler.Core.Components;
using DistributedWebCrawler.Core.Configuration;
using DistributedWebCrawler.Core.Extensions.DependencyInjection;
using DistributedWebCrawler.Core.Interfaces;
using DistributedWebCrawler.Core.Model;
using DistributedWebCrawler.Extensions.DependencyInjection.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DistributedWebCrawler.Extensions.DependencyInjection
{
    public class SchedulerBuilder : ComponentBuilder<SchedulerRequest, IngestRequest, SchedulerSettings>, ISchedulerBuilder
    {
        public SchedulerBuilder(IServiceCollection services) : base(services)
        {
            services.AddSingleton<ICrawlerComponent, SchedulerComponent>();
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
