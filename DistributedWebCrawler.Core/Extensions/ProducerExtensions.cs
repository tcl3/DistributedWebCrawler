using DistributedWebCrawler.Core.Interfaces;
using DistributedWebCrawler.Core.Models;
using DistributedWebCrawler.Core.Queue;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace DistributedWebCrawler.Core.Extensions
{
    public static class ProducerExtensions
    {
        // FIXME: This implementation needs revisiting. Might not be the best idea to try to remove the event after it is initially added
        // because this adds a lot of overhead and wouldn't happen for events that get called all the time
        private class CallbackHandler<TSuccess, TFailure>
            where TSuccess : notnull
            where TFailure : notnull, IErrorCode
        {
            private record HandlerEntry(ItemCompletedEventHandler<TSuccess>? CompletedHandler, ItemFailedEventHandler<TFailure>? FailedHandler);

            private readonly ConcurrentDictionary<Guid, HandlerEntry> _waitingHandlers;
            private readonly HashSet<IEventReceiver<TSuccess, TFailure>> _subscribedEventReceivers;

            private readonly object _callbackLock = new();

            private static readonly Lazy<CallbackHandler<TSuccess, TFailure>> _instance = new Lazy<CallbackHandler<TSuccess, TFailure>>(() => new());
            public static CallbackHandler<TSuccess, TFailure> Instance => _instance.Value;

            private CallbackHandler()
            {
                _waitingHandlers = new();
                _subscribedEventReceivers = new();
            }

            public void AddCallback(Guid id, IEventReceiver<TSuccess, TFailure> eventReceiver, 
                ItemCompletedEventHandler<TSuccess>? completedEventHandler = null, 
                ItemFailedEventHandler<TFailure>? failedEventHandler = null)
            {
                lock (_callbackLock)
                {
                    var handlerEntry = new HandlerEntry(completedEventHandler, failedEventHandler);
                    if (_waitingHandlers.TryAdd(id, handlerEntry) && _subscribedEventReceivers.Add(eventReceiver))
                    {
                        eventReceiver.OnCompletedAsync += OnCompletedAsync;
                        eventReceiver.OnFailedAsync += OnFailedAsync;
                    }                    
                }
            }

            private async Task OnCompletedAsync(object? sender, ItemCompletedEventArgs<TSuccess> e)
            {
                if (TryGetHandlerEntry(e.Id, out var handlerEntry) && handlerEntry.CompletedHandler != null)
                {
                    await handlerEntry.CompletedHandler(sender, e).ConfigureAwait(false);
                }
            }

            private async Task OnFailedAsync(object? sender, ItemFailedEventArgs<TFailure> e)
            {
                if (TryGetHandlerEntry(e.Id, out var handlerEntry) && handlerEntry.FailedHandler != null)
                {
                    await handlerEntry.FailedHandler(sender, e).ConfigureAwait(false);
                }
            }

            private bool TryGetHandlerEntry(Guid id, [NotNullWhen(returnValue: true)] out HandlerEntry? handlerEntry)
            {
                lock (_callbackLock)
                {
                    handlerEntry = null;
                    if (_waitingHandlers.TryRemove(id, out handlerEntry) && _waitingHandlers.IsEmpty)
                    {
                        foreach (var eventReceiver in _subscribedEventReceivers)
                        {
                            eventReceiver.OnCompletedAsync -= OnCompletedAsync;
                            eventReceiver.OnFailedAsync -= OnFailedAsync;
                        }

                        _subscribedEventReceivers.Clear();
                    }

                    return handlerEntry != null;
                }
            }
        }

        public static void Enqueue<TRequest, TSuccess, TFailure>(this IProducer<TRequest> producer, 
            TRequest item, 
            IEventReceiver<TSuccess, TFailure> eventReceiver, 
            ItemCompletedEventHandler<TSuccess>? completedHandler = null, 
            ItemFailedEventHandler<TFailure>? failedHandler = null) 
            where TRequest : RequestBase
            where TSuccess : notnull
            where TFailure : notnull, IErrorCode
        {
            producer.Enqueue(item);

            if (completedHandler == null && failedHandler == null)
            {
                return;
            }

            var callbackHandler = CallbackHandler<TSuccess, TFailure>.Instance;

            callbackHandler.AddCallback(item.Id, eventReceiver, completedHandler, failedHandler);
        }
        public static async Task RequeueAsync<TRequest>(this IProducer<TRequest> producer, Guid requestId, IKeyValueStore outstandingItemsStore, CancellationToken cancellationToken = default)
            where TRequest : RequestBase
        {
            var requestKey = requestId.ToString("N");
            var request = await outstandingItemsStore.GetAsync<TRequest>(requestKey).ConfigureAwait(false);
            if (request == null)
            {
                throw new KeyNotFoundException($"Request of type {typeof(TRequest).Name} and ID: {requestId} not found in KeyValueStore");
            }

            producer.Enqueue(request);
            await outstandingItemsStore.RemoveAsync(requestKey).ConfigureAwait(false);
        }
    }
}
