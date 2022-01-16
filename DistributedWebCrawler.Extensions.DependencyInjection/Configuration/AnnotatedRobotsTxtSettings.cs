using DistributedWebCrawler.Core.Configuration;
using Microsoft.Extensions.Configuration;

namespace DistributedWebCrawler.Extensions.DependencyInjection.Configuration
{
    public class AnnotatedRobotsTxtSettings : RobotsTxtSettings
    {
        [ConfigurationKeyName("MaxConcurrentRobotsRequests")]
        public override int MaxConcurrentItems { get; init; }
    }
}
