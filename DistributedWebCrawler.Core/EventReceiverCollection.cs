using DistributedWebCrawler.Core.Models;
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
            
            Scheduler = _eventReceiverFactory.Get<SchedulerSuccess, ErrorCode<SchedulerFailure>>();
            Ingester = _eventReceiverFactory.Get<IngestSuccess, IngestFailure>();
            Parser = _eventReceiverFactory.Get<ParseSuccess, ErrorCode<ParseFailure>>();
            RobotsDownloader = _eventReceiverFactory.Get<RobotsDownloaderSuccess, ErrorCode<RobotsDownloaderFailure>>();
        }

        public IEventReceiver All => _allReceivers.Value;

        public IEventReceiver<SchedulerSuccess, ErrorCode<SchedulerFailure>> Scheduler { get; }
        public IEventReceiver<IngestSuccess, IngestFailure> Ingester { get; }
        public IEventReceiver<ParseSuccess, ErrorCode<ParseFailure>> Parser { get; }
        public IEventReceiver<RobotsDownloaderSuccess, ErrorCode<RobotsDownloaderFailure>> RobotsDownloader { get; }

        public IEventReceiver<TSuccess, TFailure> OfType<TSuccess, TFailure>()
            where TSuccess : notnull
            where TFailure : notnull, IErrorCode
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
