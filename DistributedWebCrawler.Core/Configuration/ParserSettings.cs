using System.ComponentModel.DataAnnotations;

namespace DistributedWebCrawler.Core.Configuration
{
    public class ParserSettings
    {
        [Range(1, int.MaxValue)]
        public int MaxConcurrentThreads { get; set; }
    }
}
