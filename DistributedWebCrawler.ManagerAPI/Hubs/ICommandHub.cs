using DistributedWebCrawler.Core.Models;

namespace DistributedWebCrawler.ManagerAPI.Hubs
{
    public interface ICommandHub
    {
        Task Pause();
        Task Resume();
        Task PauseComponent(ComponentFilter componentFilter);
        Task ResumeComponent(ComponentFilter componentFilter);
        
        Task UpdateComponentStats(Guid componentId);
        Task UpdateAllComponentStats();
    }
}
