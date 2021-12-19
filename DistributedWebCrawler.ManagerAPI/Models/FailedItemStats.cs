using DistributedWebCrawler.Core.Interfaces;

namespace DistributedWebCrawler.ManagerAPI.Models
{
    public class FailedItemStats : ComponentStatsBase
    {
        public FailedItemStats(long sinceLastUpdate, long total) : base(sinceLastUpdate, total)
        {
        }

        public IDictionary<string, int> ErrorCounts { get; init; } = new Dictionary<string, int>();
        public IEnumerable<IErrorCode> RecentItems { get; init; } = Enumerable.Empty<IErrorCode>();
    }
}
