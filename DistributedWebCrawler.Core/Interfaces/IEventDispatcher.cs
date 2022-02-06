using DistributedWebCrawler.Core.Components;
using DistributedWebCrawler.Core.Model;
using DistributedWebCrawler.Core.Models;
using System.Threading.Tasks;

namespace DistributedWebCrawler.Core.Interfaces
{
    public interface IEventDispatcher<TSuccess, TFailure>
    {
        Task NotifyCompletedAsync(RequestBase item, NodeInfo nodeInfo, TSuccess result);
        Task NotifyFailedAsync(RequestBase item, NodeInfo nodeInfo, TFailure result);
        Task NotifyComponentStatusUpdateAsync(NodeInfo nodeInfo, ComponentStatus componentStatus);
    }
}
