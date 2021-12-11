using DistributedWebCrawler.Core.Components;
using DistributedWebCrawler.Core.Interfaces;
using System.Collections.Concurrent;

namespace DistributedWebCrawler.Core.Queue
{
    public class InMemoryEventStore<TSuccess, TFailure>
        where TSuccess : notnull
        where TFailure : notnull, IErrorCode
    {
        private readonly ConcurrentDictionary<object, object> _delegateLookup;

        public InMemoryEventStore()
        {
            _delegateLookup = new();
        }

        public ItemCompletedEventHandler<TSuccess>? OnCompletedAsyncHandler { get; set; }
        public ItemFailedEventHandler<TFailure>? OnFailedAsyncHandler { get; set; }
        public ComponentEventHandler<ComponentStatus>? OnComponentUpdateAsyncHandler { get; set; }

        public event ItemCompletedEventHandler OnCompletedAsync
        {
            add
            {
                var convertedDelegate = ConvertItemCompletedArgs(value);
                _delegateLookup.TryAdd(value, convertedDelegate);
                OnCompletedAsyncHandler += convertedDelegate;
            }
            remove
            {
                if (_delegateLookup.TryRemove(value, out var obj)
                    && obj is ItemCompletedEventHandler<TSuccess> convertedDelegate)
                {
                    OnCompletedAsyncHandler -= convertedDelegate;
                }
            }
        }

        public event ItemFailedEventHandler OnFailedAsync
        {
            add
            {
                OnFailedAsyncHandler += ConvertItemFailedArgs(value);
            }
            remove
            {
                if (_delegateLookup.TryRemove(value, out var obj)
                    && obj is ItemFailedEventHandler<TFailure> convertedDelegate)
                {
                    OnFailedAsyncHandler -= convertedDelegate;
                }
            }
        }

        private static ItemCompletedEventHandler<TSuccess> ConvertItemCompletedArgs(ItemCompletedEventHandler handler)
        {
            return (sender, args) => handler.Invoke(sender, args);
        }

        private static ItemFailedEventHandler<TFailure> ConvertItemFailedArgs(ItemFailedEventHandler handler)
        {
            return (sender, args) => handler.Invoke(sender, args);
        }
    }
}