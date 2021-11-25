using DistributedWebCrawler.Core.Extensions;
using DistributedWebCrawler.Core.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace DistributedWebCrawler.Core
{
    public class ContentStore : IContentStore
    {
        private readonly IKeyValueStore _keyValueStore;

        private const string KeyPrefix = "ContentStore:";

        public ContentStore(IKeyValueStore keyValueStore)
        {
            _keyValueStore = keyValueStore.WithKeyPrefix(KeyPrefix);
        }

        public Task<string> GetContentAsync(Guid id, CancellationToken cancellationToken)
        {
            return _keyValueStore.GetAsync(id.ToString("N"), cancellationToken);
        }

        public Task RemoveAsync(Guid id, CancellationToken cancellationToken)
        {
            return _keyValueStore.RemoveAsync(id.ToString("N"), cancellationToken);
        }

        public async Task<Guid> SaveContentAsync(string content, CancellationToken cancellationToken)
        {
            var id = Guid.NewGuid();
            await _keyValueStore.PutAsync(id.ToString("N"), content, cancellationToken).ConfigureAwait(false);

            return id;
        }
    }
}
