using DistributedWebCrawler.Core.Models;

namespace DistributedWebCrawler.ManagerAPI.Models
{
    public class ComponentStats
    {
        public ComponentStats(ComponentInfo componentInfo)
        {
            ComponentInfo = componentInfo;
        }

        public ComponentInfo ComponentInfo { get; }
        public CompletedItemStats? Completed { get; init; }
        public FailedItemStats? Failed { get; init; }
        public ComponentStatusStats? ComponentStatus { get; init; }
    }
}
