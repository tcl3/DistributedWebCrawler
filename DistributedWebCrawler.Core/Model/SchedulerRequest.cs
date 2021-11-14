using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace DistributedWebCrawler.Core.Model
{
    public class SchedulerRequest
    {
        [JsonConstructor]
        public SchedulerRequest()
        {
            Uri = default!;
        }

        public SchedulerRequest(Uri host)
        {
            if (host == null || !host.IsAbsoluteUri)
            {
                throw new UriFormatException();
            }
            Uri = host;
        }

        public Uri Uri { get; set; }
        public int CurrentCrawlDepth { get; set; }
        public IEnumerable<string> Paths { get; set; } = Enumerable.Empty<string>();
    }
}
