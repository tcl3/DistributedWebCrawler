using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace DistributedWebCrawler.Core.Models
{
    public class SchedulerRequest : RequestBase
    {
        public SchedulerRequest(Uri uri) : base(uri)
        {
        }

        public int CurrentCrawlDepth { get; set; }
        public IEnumerable<string> Paths { get; init; } = Enumerable.Empty<string>();
    }
}
