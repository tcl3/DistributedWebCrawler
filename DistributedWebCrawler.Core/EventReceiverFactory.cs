using DistributedWebCrawler.Core.Interfaces;
using System;
using System.Collections.Generic;

namespace DistributedWebCrawler.Core
{
    public class EventReceiverFactory : IEventReceiverFactory
    {
        private readonly Func<Type, object> _serviceProvider;

        public EventReceiverFactory(Func<Type, object> serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IEventReceiver<TSuccess, TFailure> Get<TSuccess, TFailure>()
            where TSuccess : notnull
            where TFailure : notnull, IErrorCode
        {
            var receiver = _serviceProvider(typeof(IEventReceiver<TSuccess, TFailure>));
            return (IEventReceiver<TSuccess, TFailure>) receiver;
        }

        public IEnumerable<IEventReceiver> GetAll()
        {
            var receivers = _serviceProvider(typeof(IEnumerable<IEventReceiver>));
            return (IEnumerable<IEventReceiver>) receivers;
        }
    }
}
