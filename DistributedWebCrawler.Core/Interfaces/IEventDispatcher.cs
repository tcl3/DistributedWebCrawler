using DistributedWebCrawler.Core.Model;
using System.Threading.Tasks;

namespace DistributedWebCrawler.Core.Interfaces
{
    public interface IEventDispatcher<TSuccess, TFailure>
    {
        Task NotifyCompletedAsync(RequestBase item, TSuccess result);
        Task NotifyFailedAsync(RequestBase item, TFailure result);
    }
}
