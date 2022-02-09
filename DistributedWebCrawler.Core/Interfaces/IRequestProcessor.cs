using DistributedWebCrawler.Core.Models;
using System.Threading;
using System.Threading.Tasks;

namespace DistributedWebCrawler.Core.Interfaces
{
    public interface IRequestProcessor<TRequest> where TRequest : RequestBase
    {
        Task<QueuedItemResult> ProcessItemAsync(TRequest item, CancellationToken cancellationToken = default);
    }
}
