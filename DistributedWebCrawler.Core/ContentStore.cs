﻿using DistributedWebCrawler.Core.Extensions;
using DistributedWebCrawler.Core.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace DistributedWebCrawler.Core
{
    public class ContentStore : IContentStore
    {
        private readonly IKeyValueStore _keyValueStore;

        private const string KeyPrefix = "ContentStore";

        public ContentStore(IKeyValueStore keyValueStore)
        {
            _keyValueStore = keyValueStore.WithKeyPrefix(KeyPrefix);
        }

        public async Task<string> GetContentAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _keyValueStore.GetAsync(id.ToString("N")).ConfigureAwait(false) ?? string.Empty;
        }

        public Task RemoveAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return _keyValueStore.RemoveAsync(id.ToString("N"));
        }

        public async Task<Guid> SaveContentAsync(string content, CancellationToken cancellationToken = default)
        {
            var id = Guid.NewGuid();
            await _keyValueStore.PutAsync(id.ToString("N"), content).ConfigureAwait(false);

            return id;
        }
    }
}
