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
            public object Content { get; }
            public DateTimeOffset CreatedAt { get; }
            public TimeSpan? ExpireAfter { get; }

            public CacheEntry(object content, TimeSpan? expireAfter = null, DateTimeOffset? createdAt = null)
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
            return GetAsync<string>(key, valueIfExpired: string.Empty, cancellationToken);
        }

        public Task RemoveAsync(string key, CancellationToken cancellationToken)
        {
            _contentLookup.TryRemove(key, out _);

            return Task.CompletedTask;
        }

        public Task PutAsync(string key, string content, CancellationToken cancellationToken, TimeSpan? expireAfter = null)
        {
            return PutAsync<string>(key, content, cancellationToken, expireAfter);
        }

        public Task PutAsync<TData>(string key, TData value, CancellationToken cancellationToken, TimeSpan? expireAfter = null)
            where TData : notnull
        {
            _contentLookup.AddOrUpdate(key, _ => new CacheEntry(value, expireAfter), (_, _) => new CacheEntry(value, expireAfter));

            return Task.FromResult(key);
        }

        public Task<TData?> GetAsync<TData>(string key, CancellationToken cancellationToken)
        {
            return GetAsync<TData>(key, valueIfExpired: default, cancellationToken);
        }

        private Task<TData?> GetAsync<TData>(string key, TData? valueIfExpired, CancellationToken cancellationToken)
        {
            if (!_contentLookup.TryGetValue(key, out var entry) || entry?.Content == null)
            {
                return Task.FromResult<TData?>(default);
            }
            else if (entry.Expired())
            {
                _contentLookup.TryRemove(key, out _);
                return Task.FromResult(valueIfExpired);
            }

            if (entry.Content is not TData entyData)
            {
                throw new InvalidOperationException($"Entry with key: '{key}' could not be converted to type: {typeof(TData).Name}");
            }

            return Task.FromResult<TData?>(entyData);
        }
    }
}
