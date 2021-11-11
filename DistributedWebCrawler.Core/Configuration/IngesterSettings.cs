using System.ComponentModel.DataAnnotations;

namespace DistributedWebCrawler.Core.Configuration
{
    public class IngesterSettings
    {
        [Range(1, int.MaxValue)]
        public int MaxDomainsToCrawl { get; set; }

        [Range(1, 100)]
        public int MaxRedirects { get; set; }
    }
}
