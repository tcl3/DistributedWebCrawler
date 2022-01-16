using DistributedWebCrawler.Core.Configuration;
using Microsoft.Extensions.Configuration;

namespace DistributedWebCrawler.Extensions.DependencyInjection.Configuration
{
    public class AnnotatedIngesterSettings : IngesterSettings
    {
        [ConfigurationKeyName("MaxDomainsToCrawl")]
        public override int MaxConcurrentItems { get; init; }
    }
}
