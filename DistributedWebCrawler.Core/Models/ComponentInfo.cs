using System;

namespace DistributedWebCrawler.Core.Models
{
    public class ComponentInfo
    {
        public string ComponentName { get; }
        public Guid ComponentId { get; }
        public Guid NodeId { get; }

        public ComponentInfo(string componentName, Guid componentId, Guid nodeId)
        {
            ComponentName = componentName;
            ComponentId = componentId;
            NodeId = nodeId;
        }
    }
}
