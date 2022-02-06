using System;

namespace DistributedWebCrawler.Core.Models
{
    public class NodeInfo
    {
        public string ComponentName { get; }
        public Guid NodeId { get; }

        public NodeInfo(string componentName, Guid nodeId)
        {
            ComponentName = componentName;
            NodeId = nodeId;
        }
    }
}
