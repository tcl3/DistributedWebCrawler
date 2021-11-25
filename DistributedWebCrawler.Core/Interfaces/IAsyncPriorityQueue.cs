using System.Threading;
using System.Threading.Tasks;

namespace DistributedWebCrawler.Core.Interfaces
{
    public interface IAsyncPriorityQueue<TData, TPriority> 
    {
        Task<bool> EnqueueAsync(TData item, TPriority priority, CancellationToken cancellationToken);
        Task<TData> DequeueAsync(CancellationToken cancellationToken);
    }
}