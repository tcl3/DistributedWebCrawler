using System;
using System.Threading;
using System.Threading.Tasks;

namespace DistributedWebCrawler.Core.Interfaces
{
    public interface IKeyValueStore
    {
        Task PutAsync(string key, string value, CancellationToken cancellationToken, TimeSpan? expireAfter = null);
        Task PutAsync<TData>(string key, TData value, CancellationToken cancellationToken, TimeSpan? expireAfter = null)
            where TData : notnull;
        Task<string?> GetAsync(string key, CancellationToken cancellationToken);
        Task<TData?> GetAsync<TData>(string key, CancellationToken cancellationToken);
        Task RemoveAsync(string key, CancellationToken cancellationToken);
    }
}
