using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace DistributedWebCrawler.Core.Models
{
    public class SchedulerRequest : RequestBase
    {
        [JsonConstructor]
        public SchedulerRequest() : base()
        {
            Uri = default!;
        }

        public SchedulerRequest(Uri host) : base()
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
