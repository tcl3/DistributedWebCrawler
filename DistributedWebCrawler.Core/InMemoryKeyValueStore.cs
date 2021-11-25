using DistributedWebCrawler.Core.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DistributedWebCrawler.Core
{
    public class InMemoryKeyValueStore : IKeyValueStore
    {
        private readonly ConcurrentDictionary<string, string> _contentLookup;

        public InMemoryKeyValueStore()
        {
            _contentLookup = new();
        }

        public Task<string> GetAsync(string key, CancellationToken cancellationToken)
        {
            if (!_contentLookup.TryGetValue(key, out var content) || content == null)
            {
                throw new KeyNotFoundException($"Key {key} not found in KeyValueStore");
            }

            return Task.FromResult(content);
        }

        public Task RemoveAsync(string key, CancellationToken cancellationToken)
        {
            if (!_contentLookup.TryRemove(key, out _))
            {
                throw new KeyNotFoundException($"Key {key} could not be removed from KeyValueStore");
            }

            return Task.CompletedTask;
        }

        public Task PutAsync(string key, string content, CancellationToken cancellationToken)
        {
            if (!_contentLookup.TryAdd(key, content))
            {
                throw new InvalidOperationException($"Failed to add key {key} to KeyValueStore");
            }

            return Task.FromResult(key);
        }
    }
}
