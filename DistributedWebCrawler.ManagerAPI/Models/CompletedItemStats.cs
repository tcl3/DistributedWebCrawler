namespace DistributedWebCrawler.ManagerAPI.Models
{
    public class CompletedItemStats : ComponentStatsBase
    {
        public CompletedItemStats(long sinceLastUpdate, long total) : base(sinceLastUpdate, total)
        {
        }
        public long? TotalBytesSinceLastUpdate { get; init; }
        public long? TotalBytes { get; init; }

        public IEnumerable<object> RecentItems { get; init; } = Enumerable.Empty<object>();
    }    
}
