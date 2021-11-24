using DistributedWebCrawler.Core.Interfaces;
using Microsoft.Extensions.Logging;
using Priority_Queue;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DistributedWebCrawler.Core.Queue
{
    public class InMemoryDateTimePriorityQueue<TData> : IAsyncPriorityQueue<TData, DateTimeOffset>
    {
        private readonly SimplePriorityQueue<TData, DateTimeOffset> _priorityQueue;
        private readonly IComparer<DateTimeOffset> _priorityComparer;
        private readonly HashSet<SemaphoreSlim> _enqueueSemaphoreList;

        private class QueueEntry
        {
            public TData Item { get; }
            public DateTimeOffset Priority { get; }
            public QueueEntry(TData item, DateTimeOffset priority)
            {
                Item = item;
                Priority = priority;
            }
        }

        public InMemoryDateTimePriorityQueue(IComparer<DateTimeOffset>? priorityComparer = null, IEqualityComparer<TData>? itemEqualityComparer = null)
        {
            priorityComparer ??= Comparer<DateTimeOffset>.Default;
            itemEqualityComparer ??= EqualityComparer<TData>.Default;

            _priorityQueue = new(priorityComparer, itemEqualityComparer);
            _priorityComparer = priorityComparer;
            _enqueueSemaphoreList = new();
        }
        private static async Task AwaitDateTime(DateTimeOffset priority, CancellationToken cancellationToken)
        {
            var delay = priority - DateTimeOffset.Now;
            if (delay > TimeSpan.Zero)
            {
                await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
            }
        }

        private bool TryGetQueueItem([NotNullWhen(returnValue: true)] out QueueEntry? entry, Predicate<DateTimeOffset>? priorityPredicate = null)
        {
            if (_priorityQueue.TryFirst(out var item)
                && _priorityQueue.TryGetPriority(item, out var innerPriority)
                && (priorityPredicate == null || priorityPredicate(innerPriority)))
            {
                entry = new QueueEntry(item, innerPriority);
                return true;
            }

            entry = null;
            return false;
        }

        private Task<TData> GetQueueItemAndAwait(SemaphoreSlim semaphore, CancellationToken cancellationToken)
        {
            Task<TData>? t1 = null;
             
            if (TryGetQueueItem(out var entry))
            {
                t1 = AwaitDateTime(entry.Priority, cancellationToken).ContinueWith(t => entry.Item, cancellationToken,
                    TaskContinuationOptions.DenyChildAttach | TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.Default);
            }
            
            var t2 = semaphore.WaitAsync(cancellationToken).ContinueWith(t => GetQueueItemAndAwait(semaphore, cancellationToken), 
                cancellationToken, TaskContinuationOptions.DenyChildAttach | TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.Default).Unwrap();

            if (t1 == null)
            {
                return t2;
            }
            return Task.WhenAny(t1, t2).Unwrap();
        }

        public async Task<TData> DequeueAsync()
        {
            var semaphore = new SemaphoreSlim(1);
            _enqueueSemaphoreList.Add(semaphore);
            try
            {
                if (TryGetQueueItem(out var entry, priority => _priorityComparer.Compare(priority, DateTimeOffset.Now) < 0)
                    && _priorityQueue.TryRemove(entry.Item))
                {
                    return entry.Item;
                }

                CancellationTokenSource cts = new();

                TData item;
                try
                {
                    do
                    {
                        item = await GetQueueItemAndAwait(semaphore, cts.Token).ConfigureAwait(false);
                    } while (!_priorityQueue.TryRemove(item));
                }
                finally
                {
                    cts.Cancel();
                }
                

                return item;
            } 
            finally
            {
                _enqueueSemaphoreList.Remove(semaphore);

            }
        }

        public Task<bool> EnqueueAsync(TData item, DateTimeOffset priority)
        {
            var success = _priorityQueue.EnqueueWithoutDuplicates(item, priority);

            if (success)
            {
                _enqueueSemaphoreList.ToList().ForEach(semaphore => semaphore.Release());
            }

            return Task.FromResult(success);
        }
    }
}
