﻿using DistributedWebCrawler.Core.Models;
using DistributedWebCrawler.Core.Extensions;
using DistributedWebCrawler.Core.Interfaces;
using System.Threading.Tasks;

namespace DistributedWebCrawler.Core.Queue
{
    public class InMemoryEventDispatcher<TSuccess, TFailure> : IEventDispatcher<TSuccess, TFailure>
        where TSuccess : notnull
        where TFailure : notnull, IErrorCode
    {
        private readonly InMemoryEventStore<TSuccess, TFailure> _eventStore;

        public InMemoryEventDispatcher(InMemoryEventStore<TSuccess, TFailure> eventStore)
        {
            _eventStore = eventStore;
        }

        public async Task NotifyCompletedAsync(RequestBase item, ComponentInfo nodeInfo, TSuccess result)
        {
            if (_eventStore.OnCompletedAsyncHandler != null)
            {
                await _eventStore.OnCompletedAsyncHandler(this, new ItemCompletedEventArgs<TSuccess>(item.Id, nodeInfo, result)).ConfigureAwait(false);
            }
        }

        public async Task NotifyFailedAsync(RequestBase item, ComponentInfo nodeInfo, TFailure result)
        {
            if (_eventStore.OnFailedAsyncHandler != null)
            {
                await _eventStore.OnFailedAsyncHandler(this, new ItemFailedEventArgs<TFailure>(item.Id, nodeInfo, result)).ConfigureAwait(false);
            }
        }

        public async Task NotifyComponentStatusUpdateAsync(ComponentInfo nodeInfo, ComponentStatus componentStatus)
        {
            if (_eventStore.OnComponentUpdateAsyncHandler != null)
            {
                await _eventStore.OnComponentUpdateAsyncHandler(this, new ComponentEventArgs<ComponentStatus>(nodeInfo, componentStatus)).ConfigureAwait(false);
            }
        }
    }
}