using DistributedWebCrawler.Core.Interfaces;

namespace DistributedWebCrawler.Core.Extensions
{
    public static class KeyValueStoreExtensions
    {
        public static IKeyValueStore WithKeyPrefix(this IKeyValueStore keyValueStore, string prefix)
        {
            return new KeyValueStorePrefixDecorator(keyValueStore, prefix);
        }
    }
}
