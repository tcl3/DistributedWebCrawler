using DistributedWebCrawler.Core.Enums;
using DistributedWebCrawler.ManagerAPI.Models;

namespace DistributedWebCrawler.ManagerAPI.Hubs
{
    public interface IComponentEventsHub
    {
        Task OnComponentUpdate(string componentName, ComponentStatusStats data);
        Task OnCompleted(string componentName, CompletedItemStats data);
        Task OnFailed(string componentName, FailedItemStats data);
    }
}
