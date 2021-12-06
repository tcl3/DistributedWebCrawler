using DistributedWebCrawler.Core.Components;
using DistributedWebCrawler.Core.Interfaces;
using DistributedWebCrawler.Core.Model;
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
            
            Scheduler = _eventReceiverFactory.Get<SchedulerSuccess, SchedulerFailure>();
            Ingester = _eventReceiverFactory.Get<IngestSuccess, IngestFailure>();
            Parser = _eventReceiverFactory.Get<ParseSuccess, ParseFailure>();
            RobotsDownloader = _eventReceiverFactory.Get<RobotsDownloaderSuccess, RobotsDownloaderFailure>();
        }

        public IEventReceiver All => _allReceivers.Value;

        public IEventReceiver<SchedulerSuccess, SchedulerFailure> Scheduler { get; }
        public IEventReceiver<IngestSuccess, IngestFailure> Ingester { get; }
        public IEventReceiver<ParseSuccess, ParseFailure> Parser { get; }
        public IEventReceiver<RobotsDownloaderSuccess, RobotsDownloaderFailure> RobotsDownloader { get; }

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
