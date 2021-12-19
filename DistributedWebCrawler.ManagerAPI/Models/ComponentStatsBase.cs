namespace DistributedWebCrawler.ManagerAPI.Models
{
    public class ComponentStatsBase
    {
        public ComponentStatsBase(long sinceLastUpdate, long total)
        {
            SinceLastUpdate = sinceLastUpdate;
            Total = total;
        }

        public long SinceLastUpdate { get; init;  }
        public long Total { get; }
    }
}
