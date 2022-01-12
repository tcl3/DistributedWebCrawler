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
                CreatedAt = createdAt ?? SystemClock.DateTimeOffsetNow();
            }

            public bool Expired()
            {                
                return ExpireAfter.HasValue && (CreatedAt + ExpireAfter) < SystemClock.DateTimeOffsetNow();
            }
        }

        private readonly ConcurrentDictionary<string, CacheEntry> _contentLookup;

        public InMemoryKeyValueStore()
        {
            _contentLookup = new();
        }

        public Task<string?> GetAsync(string key)
        {
            return GetAsync<string>(key, valueIfExpired: string.Empty);
        }

        public Task RemoveAsync(string key)
        {
            _contentLookup.TryRemove(key, out _);

            return Task.CompletedTask;
        }

        public Task PutAsync(string key, string content, TimeSpan? expireAfter = null)
        {
            return PutAsync<string>(key, content, expireAfter);
        }

        public Task PutAsync<TData>(string key, TData value, TimeSpan? expireAfter = null)
            where TData : notnull
        {
            _contentLookup.AddOrUpdate(key, _ => new CacheEntry(value, expireAfter), (_, _) => new CacheEntry(value, expireAfter));

            return Task.FromResult(key);
        }

        public Task<TData?> GetAsync<TData>(string key)
        {
            return GetAsync<TData>(key, valueIfExpired: default);
        }

        private Task<TData?> GetAsync<TData>(string key, TData? valueIfExpired)
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