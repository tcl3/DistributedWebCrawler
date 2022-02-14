using DistributedWebCrawler.Core.Enums;

namespace DistributedWebCrawler.Core.Models
{
    public class ComponentStatus
    {
        public int TasksInUse { get; init; } 
        public int MaxConcurrentTasks { get; init; }
        public int QueueCount { get; init; }
        public CrawlerComponentStatus CurrentStatus { get; init; }
        public NodeStatus NodeStatus { get; }

        public ComponentStatus(NodeStatus nodeStatus)
        {
            NodeStatus = nodeStatus;
        }
    };
}