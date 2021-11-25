using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DistributedWebCrawler.Core.Configuration
{
    public class IngesterSettings : TaskQueueSettings
    {
        [Range(1, int.MaxValue)]
        public int MaxDomainsToCrawl { get; init; }

        [Range(1, 100)]
        public int MaxRedirects { get; init; }

        [Range(1, long.MaxValue)]
        public long? MaxContentLengthBytes { get; init; }

        public IEnumerable<string>? IncludeMediaTypes { get; init; }
        public IEnumerable<string>? ExcludeMediaTypes { get; init; }

        internal override int MaxConcurrentItems => MaxDomainsToCrawl;
    }
}
