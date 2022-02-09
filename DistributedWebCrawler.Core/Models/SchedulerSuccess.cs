using System;
using System.Collections.Generic;

namespace DistributedWebCrawler.Core.Models
{
    public class SchedulerSuccess
    {
        public Uri Uri { get; init; }
        public IEnumerable<string> AddedPaths { get; init; }

        public SchedulerSuccess(Uri uri, IEnumerable<string> addedPaths)
        {
            Uri = uri;
            AddedPaths = addedPaths;
        }
    }
}
