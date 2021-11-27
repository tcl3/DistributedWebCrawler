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
        private class CacheEntry
        {
            public string Content { get; }
            public DateTimeOffset CreatedAt { get; }
            public TimeSpan? ExpireAfter { get; }

            public CacheEntry(string content, TimeSpan? expireAfter = null, DateTimeOffset? createdAt = null)
            {
                Content = content;
                ExpireAfter = expireAfter;
                CreatedAt = createdAt ?? DateTimeOffset.Now;
            }

            public bool Expired()
            {                
                return ExpireAfter.HasValue && (CreatedAt + ExpireAfter) < DateTimeOffset.Now;
            }
        }

        private readonly ConcurrentDictionary<string, CacheEntry> _contentLookup;

        public InMemoryKeyValueStore()
        {
            _contentLookup = new();
        }

        public Task<string?> GetAsync(string key, CancellationToken cancellationToken)
        {
            if (!_contentLookup.TryGetValue(key, out var entry) || entry?.Content == null)
            {
                return Task.FromResult<string?>(null);
            }
            else if (entry.Expired())
            {
                _contentLookup.TryRemove(key, out _);
                return Task.FromResult<string?>(string.Empty);
            }

            return Task.FromResult<string?>(entry.Content);
        }

        public Task RemoveAsync(string key, CancellationToken cancellationToken)
        {
            _contentLookup.TryRemove(key, out _);

            return Task.CompletedTask;
        }

        public Task PutAsync(string key, string content, CancellationToken cancellationToken, TimeSpan? expireAfter = null)
        {
            _contentLookup.AddOrUpdate(key, _ => new CacheEntry(content, expireAfter), (_, _) => new CacheEntry(content, expireAfter));

            return Task.FromResult(key);
        }
    }
}
