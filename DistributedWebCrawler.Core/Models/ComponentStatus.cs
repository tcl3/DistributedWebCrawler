namespace DistributedWebCrawler.Core.Components
{
    public class ComponentStatus
    {
        public int TasksInUse { get; init; } 
        public int MaxConcurrentTasks { get; init; }
        public int QueueCount { get; init; }
    };
}