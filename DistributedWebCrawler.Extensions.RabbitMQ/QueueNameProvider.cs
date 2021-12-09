using DistributedWebCrawler.Core;
using DistributedWebCrawler.Core.Extensions;
using System.Collections.Concurrent;

namespace DistributedWebCrawler.Extensions.RabbitMQ
{
    public class QueueNameProvider<TSuccess, TFailure>
    {
        private readonly ConcurrentDictionary<Type, string> _queueNameLookup;

        private readonly string _queueNamePrefix;
        
        private const string QueueNameSuffix = "Notifier";

        public QueueNameProvider(ComponentNameProvider<TSuccess, TFailure> componentNameProvider)
        {
            _queueNameLookup = new();
            _queueNamePrefix = componentNameProvider.GetComponentNameOrDefault(() => Guid.NewGuid().ToString("N"));
        }

        public string GetQueueName<TData>()
        {
            return _queueNameLookup.GetOrAdd(typeof(TData), GetQueueNameFromType);
        }

        private string GetQueueNameFromType(Type type)
        {
            var commonPrefix = _queueNamePrefix.GetCommonPrefix(type.Name);

            var typeName = type.Name[commonPrefix.Length..];

            return _queueNamePrefix + typeName + QueueNameSuffix;
        }
    }
}
