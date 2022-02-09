namespace DistributedWebCrawler.ManagerAPI.Models
{
    public class ComponentStatusStats : ComponentStatsBase
    {
        public ComponentStatusStats(long sinceLastUpdate, long total) : base(sinceLastUpdate, total)
        {
        }

        public int AverageTasksInUse { get; init; }
        public int AverageQueueCount { get; init; }
        public int MaxTasks { get; init; }

        public Dictionary<Guid, NodeStatusStats> NodeStatus { get; init; } = new Dictionary<Guid, NodeStatusStats>();
    }

}
