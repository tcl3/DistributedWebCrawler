using System;
using System.Threading.Tasks;

namespace DistributedWebCrawler.Core.Interfaces
{
    public interface IContentStore
    {
        Task<Guid> SaveContentAsync(string content);
        Task<string> GetContentAsync(Guid id);
        Task RemoveAsync(Guid id);
    }
}
