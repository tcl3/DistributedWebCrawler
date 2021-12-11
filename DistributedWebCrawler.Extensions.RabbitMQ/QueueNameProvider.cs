using DistributedWebCrawler.Core;
using DistributedWebCrawler.Core.Interfaces;
using System.Collections.Concurrent;

namespace DistributedWebCrawler.Extensions.RabbitMQ
{
    public class QueueNameProvider<TSuccess, TFailure>
        where TFailure : IErrorCode
    {
        private readonly ConcurrentDictionary<Type, string> _queueNameLookup;

        private readonly string _queueNamePrefix;
        
        private const string QueueNameSuffix = "Notifier";

        public QueueNameProvider(ComponentNameProvider componentNameProvider)
        {
            _queueNameLookup = new();
            _queueNamePrefix = componentNameProvider.GetComponentNameOrDefault<TSuccess, TFailure>(() => Guid.NewGuid().ToString("N"));
        }

        public string GetQueueName<TData>()
        {
            return _queueNameLookup.GetOrAdd(typeof(TData), GetQueueNameFromType);
        }

        private string GetQueueNameFromType(Type type)
        {
            string typeName;
            if (type == typeof(TSuccess)) 
            {
                typeName = "Success";
            }
            else if (type == typeof(TFailure))
            {
                typeName = "Failure";
            } 
            else
            {
                typeName = type.Name;
            }

            return _queueNamePrefix + typeName + QueueNameSuffix;
        }
    }
}