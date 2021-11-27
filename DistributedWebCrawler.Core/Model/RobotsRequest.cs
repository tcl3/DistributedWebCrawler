using System;

namespace DistributedWebCrawler.Core.Model
{
    public class RobotsRequest : RequestBase
    {
        public RobotsRequest(Uri uri, SchedulerRequest schedulerRequest) : base()
        {
            Uri = uri;
            SchedulerRequest = schedulerRequest;
        }

        public Uri Uri { get; init; }

        // TODO: Pause / Resume functionality for queued tasks, so we don't have to pass an entire request here
        public SchedulerRequest SchedulerRequest { get; init; }
    }
}
