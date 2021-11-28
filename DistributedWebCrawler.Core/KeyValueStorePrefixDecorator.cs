using DistributedWebCrawler.Core.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace DistributedWebCrawler.Core
{
    internal class KeyValueStorePrefixDecorator : IKeyValueStore
    {
        private readonly IKeyValueStore _inner;
        private readonly string _prefix;

        private const string PrefixSeperator = ":";

        public KeyValueStorePrefixDecorator(IKeyValueStore inner, string prefix)
        {
            if (string.IsNullOrEmpty(prefix))
            {
                throw new ArgumentNullException(nameof(prefix));
            }

            _inner = inner;
            _prefix = prefix + PrefixSeperator;
        }

        public Task<string?> GetAsync(string key, CancellationToken cancellationToken)
        {
            return _inner.GetAsync(_prefix + key, cancellationToken);
        }

        public Task PutAsync(string key, string value, CancellationToken cancellationToken, TimeSpan? expireAfter = null)
        {
            return _inner.PutAsync(_prefix + key, value, cancellationToken, expireAfter);
        }

        public Task RemoveAsync(string key, CancellationToken cancellationToken)
        {
            return _inner.RemoveAsync(_prefix + key, cancellationToken);
        }

        public Task PutAsync<TData>(string key, TData value, CancellationToken cancellationToken, TimeSpan? expireAfter = null) 
            where TData : notnull
        {
            return _inner.PutAsync(_prefix + key, value, cancellationToken, expireAfter);
        }

        public Task<TData?> GetAsync<TData>(string key, CancellationToken cancellationToken)
        {
            return _inner.GetAsync<TData>(_prefix + key, cancellationToken);
        }
    }
}