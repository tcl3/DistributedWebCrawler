using DistributedWebCrawler.Core.Models;

namespace DistributedWebCrawler.ManagerAPI.Hubs
{
    public interface ICommandHub
    {
        Task Pause(ComponentFilter? componentFilter = null);
        Task Resume(ComponentFilter? componentFilter = null);
    }
}
