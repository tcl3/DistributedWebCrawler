﻿using DistributedWebCrawler.Core.Components;
using DistributedWebCrawler.Core.Interfaces;
using System.Collections.Concurrent;

namespace DistributedWebCrawler.Core.Queue
{
    public class InMemoryEventStore<TSuccess, TFailure>
    {
        private readonly ConcurrentDictionary<ItemCompletedEventHandler, object> _delegateLookup;

        public InMemoryEventStore()
        {
            _delegateLookup = new();
        }

        public ItemCompletedEventHandler<TSuccess>? OnCompletedAsyncHandler { get; set; }
        public ItemCompletedEventHandler<TFailure>? OnFailedAsyncHandler { get; set; }
        public AsyncEventHandler<ComponentStatus>? OnComponentUpdateAsyncHandler { get; set; }

        public event ItemCompletedEventHandler OnCompletedAsync
        {
            add
            {
                var convertedDelegate = ConvertArgs<TSuccess>(value);
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

        public event ItemCompletedEventHandler OnFailedAsync
        {
            add
            {
                OnFailedAsyncHandler += ConvertArgs<TFailure>(value);
            }
            remove
            {
                if (_delegateLookup.TryRemove(value, out var obj)
                    && obj is ItemCompletedEventHandler<TFailure> convertedDelegate)
                {
                    OnFailedAsyncHandler -= convertedDelegate;
                }
            }
        }

        private static ItemCompletedEventHandler<T> ConvertArgs<T>(ItemCompletedEventHandler handler)
        {
            return (sender, args) => handler.Invoke(sender, new ItemCompletedEventArgs(args.Id, args.Result!));
        }
    }
}