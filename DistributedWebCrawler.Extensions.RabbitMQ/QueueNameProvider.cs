using System.Collections.Concurrent;

namespace DistributedWebCrawler.Extensions.RabbitMQ
{
    public class QueueNameProvider<TSuccess, TFailure>
    {
        private readonly ConcurrentDictionary<Type, string> _queueNameLookup;

        private static readonly string QueueNamePrefix;
        private const string QueueNameSuffix = "Notifier";

        static QueueNameProvider()
        {
            var commonPrefix = GetCommonPrefix(typeof(TSuccess).Name, typeof(TFailure).Name);
            if (commonPrefix == string.Empty)
            {
                QueueNamePrefix = Guid.NewGuid().ToString("N") + '-';
            }
            else
            {
                QueueNamePrefix = commonPrefix;
            }
        }

        public QueueNameProvider()
        {
            _queueNameLookup = new();
        }

        public string GetQueueName<TData>()
        {
            return _queueNameLookup.GetOrAdd(typeof(TData), GetQueueNameFromType);
        }

        private static string GetQueueNameFromType(Type type)
        {
            string prefix = QueueNamePrefix;
            if (prefix.EndsWith('e'))
            {
                prefix += 'r';
            }
            else if (!prefix.EndsWith("er"))
            {

                prefix += "er";
            }
            var typeName = type.Name;
            if (typeName.StartsWith(QueueNamePrefix))
            {
                typeName = typeName[QueueNamePrefix.Length..];
            }

            return prefix + typeName + QueueNameSuffix;
        }

        private static string GetCommonPrefix(string first, string second)
        {
            if (first[0] != second[0])
            {
                return string.Empty;
            }

            if (first.Length > second.Length)
            {
                var tmp = first;
                first = second;
                second = tmp;
            }
            
            for (var i = 1; i < first.Length; i++)
            {
                if (first[i] != second[i])
                {
                    return first[..i];
                }
            }

            return first;
        }
    }
}
