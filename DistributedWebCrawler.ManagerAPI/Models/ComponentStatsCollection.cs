namespace DistributedWebCrawler.ManagerAPI.Models
{
    public class ComponentStatsCollection
    {
        public IEnumerable<ComponentStats> ComponentStats { get; init; } = Enumerable.Empty<ComponentStats>();
        public Dictionary<Guid, NodeStatusStats> NodeStatus { get; init; } = new Dictionary<Guid, NodeStatusStats>();
    }
}
