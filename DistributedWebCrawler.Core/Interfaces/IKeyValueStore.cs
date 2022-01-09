using System;
using System.Threading;
using System.Threading.Tasks;

namespace DistributedWebCrawler.Core.Interfaces
{
    public interface IKeyValueStore
    {
        Task PutAsync(string key, string value, TimeSpan? expireAfter = null);
        Task PutAsync<TData>(string key, TData value, TimeSpan? expireAfter = null)
            where TData : notnull;
        Task<string?> GetAsync(string key);
        Task<TData?> GetAsync<TData>(string key);
        Task RemoveAsync(string key);
    }
}
