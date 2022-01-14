using DistributedWebCrawler.Core.Constants;
using System;
using System.ComponentModel.DataAnnotations;

namespace DistributedWebCrawler.Core.Configuration
{
    public abstract class TaskQueueSettings
    {
        [Range(1, 1 * TimeConstants.SecondsPerDay)]
        public int QueueItemTimeoutSeconds { get; init; }

        internal abstract int MaxConcurrentItems { get; }
    }
}
