using DistributedWebCrawler.Core.Interfaces;
using System.Collections.Generic;

namespace DistributedWebCrawler.Core
{
    public class CompositeEventReceiver : IEventReceiver
    {
        private readonly IEnumerable<IEventReceiver> _eventReceivers;

        public CompositeEventReceiver(IEnumerable<IEventReceiver> eventReceivers)
        {
            _eventReceivers = eventReceivers;
        }

        public event ItemCompletedEventHandler OnCompletedAsync
        {
            add
            {
                foreach (var eventReceiver in _eventReceivers)
                {
                    eventReceiver.OnCompletedAsync += value;
                }
            }
            remove
            {
                foreach (var eventReceiver in _eventReceivers)
                {
                    eventReceiver.OnCompletedAsync -= value;
                }
            }
        }
        public event ItemCompletedEventHandler OnFailedAsync
        {
            add
            {
                foreach (var eventReceiver in _eventReceivers)
                {
                    eventReceiver.OnFailedAsync += value;
                }
            }
            remove
            {
                foreach (var eventReceiver in _eventReceivers)
                {
                    eventReceiver.OnFailedAsync -= value;
                }
            }
        }
    }
}
