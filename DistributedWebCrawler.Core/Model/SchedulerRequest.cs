using System;
using System.Collections.Generic;
using System.Linq;

namespace DistributedWebCrawler.Core.Model
{
    public class SchedulerRequest
    {
        public SchedulerRequest(Uri host)
        {
            if (host == null || !host.IsAbsoluteUri)
            {
                throw new UriFormatException();
            }
            Uri = host;
        }

        public Uri Uri { get; }
        public int CurrentCrawlDepth { get; set; }
        public IEnumerable<string> Paths { get; set; } = Enumerable.Empty<string>();
    }
}
