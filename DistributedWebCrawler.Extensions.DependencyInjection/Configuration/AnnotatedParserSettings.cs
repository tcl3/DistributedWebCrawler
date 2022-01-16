using DistributedWebCrawler.Core.Configuration;
using Microsoft.Extensions.Configuration;

namespace DistributedWebCrawler.Extensions.DependencyInjection.Configuration
{
    public class AnnotatedParserSettings : ParserSettings
    {
        [ConfigurationKeyName("MaxConcurrentThreads")]
        public override int MaxConcurrentItems { get; init; }
    }
}
