using System;
using System.Threading;
using System.Threading.Tasks;

namespace DistributedWebCrawler.Core.Interfaces
{
    public interface IContentStore
    {
        Task<Guid> SaveContentAsync(string content, CancellationToken cancellationToken = default);
        Task<string> GetContentAsync(Guid id, CancellationToken cancellationToken = default);
        Task RemoveAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
