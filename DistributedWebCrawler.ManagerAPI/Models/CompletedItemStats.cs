namespace DistributedWebCrawler.ManagerAPI.Models
{
    public class CompletedItemStats : ComponentStatsBase
    {
        public CompletedItemStats(long sinceLastUpdate, long total) : base(sinceLastUpdate, total)
        {
        }
        public long? TotalBytesIngestedSinceLastUpdate { get; init; }
        public long? TotalBytesIngested { get; init; }

        public IEnumerable<object> RecentItems { get; init; } = Enumerable.Empty<object>();
    }    
}
