using DistributedWebCrawler.Core;
using DistributedWebCrawler.Core.Extensions;
using DistributedWebCrawler.Core.Interfaces;
using System.Collections.Concurrent;

namespace DistributedWebCrawler.Extensions.RabbitMQ
{
    public class ExchangeNameProvider<TSuccess, TFailure>
        where TFailure : IErrorCode
    {
        private readonly ConcurrentDictionary<Type, string> _exchangeNameLookup;

        private readonly string _exchangeNamePrefix;
        
        private const string ExchangeNameSuffix = "Notifier";

        public ExchangeNameProvider(IComponentNameProvider componentNameProvider)
        {
            _exchangeNameLookup = new();
            _exchangeNamePrefix = componentNameProvider.GetComponentNameOrDefault<TSuccess, TFailure>(() => Guid.NewGuid().ToString("N"));
        }

        public string GetExchangeName<TData>()
        {
            return _exchangeNameLookup.GetOrAdd(typeof(TData), GetExchangeNameFromType);
        }

        private string GetExchangeNameFromType(Type type)
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

            return _exchangeNamePrefix + typeName + ExchangeNameSuffix;
        }
    }
}