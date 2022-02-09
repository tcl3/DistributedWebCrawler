using DistributedWebCrawler.Core.Models;
using System.Threading.Tasks;

namespace DistributedWebCrawler.Core.Interfaces
{
    public interface IEventDispatcher<TSuccess, TFailure>
    {
        Task NotifyCompletedAsync(RequestBase item, ComponentInfo nodeInfo, TSuccess result);
        Task NotifyFailedAsync(RequestBase item, ComponentInfo nodeInfo, TFailure result);
        Task NotifyComponentStatusUpdateAsync(ComponentInfo nodeInfo, ComponentStatus componentStatus);
    }
}
