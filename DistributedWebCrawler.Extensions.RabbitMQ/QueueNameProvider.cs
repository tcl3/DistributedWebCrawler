using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DistributedWebCrawler.Extensions.RabbitMQ
{
    public class QueueNameProvider<TSuccess, TFailure>
    {
        private readonly ConcurrentDictionary<Type, string> _queueNameLookup;

        private static readonly string QueueNamePrefix;
        private const string QueueNameSuffix = "-Notifier";

        static QueueNameProvider()
        {
            var commonPrefix = GetCommonPrefix(typeof(TSuccess).Name, typeof(TFailure).Name);
            if (commonPrefix == string.Empty)
            {
                QueueNamePrefix = Guid.NewGuid().ToString("N") + '-';
            }
            else
            {
                if (commonPrefix.EndsWith('e'))
                {
                    commonPrefix += 'r';
                }
                else if (!commonPrefix.EndsWith("er"))
                {
                    
                    commonPrefix += "er";
                }
                QueueNamePrefix = commonPrefix  + '-';
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

        private string GetQueueNameFromType(Type type)
        {
            return QueueNamePrefix + type.Name + QueueNameSuffix;
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
