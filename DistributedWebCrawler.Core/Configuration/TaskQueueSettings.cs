using System;
using System.ComponentModel.DataAnnotations;

namespace DistributedWebCrawler.Core.Configuration
{
    public abstract class TaskQueueSettings
    {
        [Range(1, int.MaxValue, ErrorMessage = nameof(QueueItemTimeoutSeconds) + " must be a positive integer")]
        public int QueueItemTimeoutSeconds { get; init; }

        internal abstract int MaxConcurrentItems { get; }
    }
}
