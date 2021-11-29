using DistributedWebCrawler.Core.Model;
using System.Threading.Tasks;

namespace DistributedWebCrawler.Core.Interfaces
{
    public interface IEventDispatcher<TRequest, TResult> 
        where TRequest : RequestBase
    {
        Task NotifyCompletedAsync(TRequest item, TaskStatus status, TResult? result);
    }
}
