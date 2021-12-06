using DistributedWebCrawler.Core.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DistributedWebCrawler.Core
{
    public class EventReceiverCollection : IEnumerable<IEventReceiver>
    {
        private readonly Lazy<IEventReceiver> _allReceivers;
        private readonly IEventReceiverFactory _eventReceiverFactory;

        public EventReceiverCollection(IEventReceiverFactory eventReceiverFactory)
        {
            _allReceivers = new Lazy<IEventReceiver>(() => new CompositeEventReceiver(this.ToList()));
            _eventReceiverFactory = eventReceiverFactory;
        }

        public IEventReceiver All => _allReceivers.Value;

        public IEventReceiver<TSuccess, TFailure> OfType<TSuccess, TFailure>()
        {
            return _eventReceiverFactory.Get<TSuccess, TFailure>();
        }

        public IEnumerator<IEventReceiver> GetEnumerator()
        {
            return _eventReceiverFactory.GetAll().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
