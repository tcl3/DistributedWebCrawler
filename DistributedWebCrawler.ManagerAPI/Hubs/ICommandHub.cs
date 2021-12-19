namespace DistributedWebCrawler.ManagerAPI.Hubs
{
    public interface ICommandHub
    {
        Task Pause();
        Task Resume();
    }
}
