using DistributedWebCrawler.Core.Configuration;
using Microsoft.Extensions.Configuration;

namespace DistributedWebCrawler.Extensions.DependencyInjection.Configuration
{
    public class AnnotatedSchedulerSettings : SchedulerSettings
    {
        [ConfigurationKeyName("MaxConcurrentSchedulerRequests")]
        public override int MaxConcurrentItems { get; init; }
    }
}
