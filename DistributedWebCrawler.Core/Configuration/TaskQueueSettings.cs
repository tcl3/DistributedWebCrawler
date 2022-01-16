using DistributedWebCrawler.Core.Constants;
using System;
using System.ComponentModel.DataAnnotations;

namespace DistributedWebCrawler.Core.Configuration
{
    public abstract class TaskQueueSettings
    {
        [Range(1, 1 * TimeConstants.SecondsPerDay)]
        public virtual int QueueItemTimeoutSeconds { get; init; }

        [Range(1, int.MaxValue)]
        public virtual int MaxConcurrentItems { get; init; }
    }
}
