namespace DistributedWebCrawler.ManagerAPI.Models
{
    public class NodeStatusStats
    {
        public long TotalBytesDownloaded { get; init; }
        public long TotalBytesUploaded { get; init; }
        public long TotalBytesDownloadedSinceLastUpdate { get; init; }
        public long TotalBytesUploadedSinceLastUpdate { get; init; }
    }
}
