using System;
using System.Collections.Generic;

namespace DistributedWebCrawler.Core.Model
{
    public record SchedulerSuccess(Uri Uri, IEnumerable<string> AddedPaths);
}
