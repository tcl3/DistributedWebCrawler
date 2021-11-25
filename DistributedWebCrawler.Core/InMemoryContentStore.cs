using DistributedWebCrawler.Core.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DistributedWebCrawler.Core
{
    public class InMemoryContentStore : IContentStore
    {
        private readonly ConcurrentDictionary<Guid, string> _contentLookup;

        public InMemoryContentStore()
        {
            _contentLookup = new();
        }

        public Task<string> GetContentAsync(Guid id, CancellationToken cancellationToken)
        {
            if (!_contentLookup.TryGetValue(id, out var content) || content == null)
            {
                throw new KeyNotFoundException($"Key {id} not found in content store");
            }

            return Task.FromResult(content);
        }

        public Task RemoveAsync(Guid id, CancellationToken cancellationToken)
        {
            if (!_contentLookup.TryRemove(id, out _))
            {
                throw new KeyNotFoundException($"Key {id} could not be removed from content store");
            }

            return Task.CompletedTask;
        }

        public Task<Guid> SaveContentAsync(string content, CancellationToken cancellationToken)
        {
            var id = Guid.NewGuid();
            if (!_contentLookup.TryAdd(id, content))
            {
                throw new InvalidOperationException($"Failed to add key {id} to content store");
            }

            return Task.FromResult(id);
        }
    }
}
