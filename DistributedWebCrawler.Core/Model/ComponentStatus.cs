namespace DistributedWebCrawler.Core.Components
{
    public record ComponentStatus(int TasksInUse, int MaxConcurrentTasks);
}