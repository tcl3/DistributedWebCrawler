using System;

namespace DistributedWebCrawler.Core.Models
{
    public class NodeStatus
    {
        public Guid NodeId { get; }
        public long TotalBytesDownloaded { get; }
        public long TotalBytesUploaded { get;  }

        public NodeStatus(Guid nodeId, long totalBytesDownloaded, long totalBytesUploaded)
        {
            NodeId = nodeId;
            TotalBytesDownloaded = totalBytesDownloaded;
            TotalBytesUploaded = totalBytesUploaded;
        }
    }
}
