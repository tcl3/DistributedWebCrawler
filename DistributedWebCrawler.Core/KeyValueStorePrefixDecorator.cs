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

        public KeyValueStorePrefixDecorator(IKeyValueStore inner, string prefix)
        {
            if (string.IsNullOrEmpty(prefix))
            {
                throw new ArgumentNullException(nameof(prefix));
            }

            _inner = inner;
            _prefix = prefix;
        }

        public Task<string> GetAsync(string key, CancellationToken cancellationToken)
        {
            return _inner.GetAsync(_prefix + key, cancellationToken);
        }

        public Task PutAsync(string key, string value, CancellationToken cancellationToken)
        {
            return _inner.PutAsync(_prefix + key, value, cancellationToken);
        }

        public Task RemoveAsync(string key, CancellationToken cancellationToken)
        {
            return _inner.RemoveAsync(_prefix + key, cancellationToken);
        }
    }
}