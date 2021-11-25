using System;
using System.Threading;
using System.Threading.Tasks;

namespace DistributedWebCrawler.Core.Interfaces
{
    public interface IContentStore
    {
        Task<Guid> SaveContentAsync(string content, CancellationToken cancellationToken);
        Task<string> GetContentAsync(Guid id, CancellationToken cancellationToken);
        Task RemoveAsync(Guid id, CancellationToken cancellationToken);
    }
}
