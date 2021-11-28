using System;

namespace DistributedWebCrawler.Core.Model
{
    public class RobotsRequest : RequestBase
    {
        public RobotsRequest(Uri uri, Guid schedulerRequestId) : base()
        {
            Uri = uri;
            SchedulerRequestId = schedulerRequestId;
        }

        public Uri Uri { get; init; }

        public Guid SchedulerRequestId { get; init; }
    }
}
