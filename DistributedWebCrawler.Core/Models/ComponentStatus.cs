namespace DistributedWebCrawler.Core.Models
{
    public class ComponentStatus
    {
        public int TasksInUse { get; init; } 
        public int MaxConcurrentTasks { get; init; }
        public int QueueCount { get; init; }
    };
}