using System.ComponentModel.DataAnnotations;

namespace DistributedWebCrawler.Core.Configuration
{
    public class ParserSettings : TaskQueueSettings
    {
        [Range(1, int.MaxValue)]
        public int MaxConcurrentThreads { get; init; }

        internal override int MaxConcurrentItems => MaxConcurrentThreads;
    }
}
