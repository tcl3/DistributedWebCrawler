using DistributedWebCrawler.Core.Enums;
using DistributedWebCrawler.ManagerAPI.Models;

namespace DistributedWebCrawler.ManagerAPI.Hubs
{
    public interface IComponentEventsHub
    {
        Task OnComponentUpdate(ComponentStatsCollection componentStats);
        Task OnAllComponentsPaused();
    }
}
