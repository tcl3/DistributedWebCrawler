using System;

namespace DistributedWebCrawler.Core.Models
{
    public class RobotsRequest : RequestBase
    {
        public RobotsRequest(Uri uri, Guid schedulerRequestId) : base(uri)
        {
            SchedulerRequestId = schedulerRequestId;
        }

        public Guid SchedulerRequestId { get; init; }
    }
}
