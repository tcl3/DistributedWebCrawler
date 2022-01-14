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

        // FIXME: this has to be an int because RangeAttribute does not support the full range of long values
        // Could implement a custom attribute to allow ContentLength > 2GB.
        [Range(1, int.MaxValue)]
        public int? MaxContentLengthBytes { get; init; }

        public IEnumerable<string>? IncludeMediaTypes { get; init; }
        public IEnumerable<string>? ExcludeMediaTypes { get; init; }

        internal override int MaxConcurrentItems => MaxDomainsToCrawl;
    }
}
