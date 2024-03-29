﻿using DistributedWebCrawler.Core.Interfaces;
using Priority_Queue;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DistributedWebCrawler.Core.Queue
{
    public class InMemoryDateTimePriorityQueue<TData> : IAsyncDateTimePriorityQueue<TData>
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
        private static async Task AwaitDateTime(DateTimeOffset priority, CancellationToken cancellationToken = default)
        {
            var delay = priority - SystemClock.DateTimeOffsetNow();
            if (delay > TimeSpan.Zero)
            {
                await SystemClock.DelayAsync(delay, cancellationToken).ConfigureAwait(false);
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

        private Task<TData> GetQueueItemAndAwait(SemaphoreSlim semaphore, CancellationToken cancellationToken = default)
        {             
            var awaitSemaphoreTask = semaphore.WaitAsync(cancellationToken)
                    .ContinueWith(t => GetQueueItemAndAwait(semaphore, cancellationToken), cancellationToken,
                    TaskContinuationOptions.DenyChildAttach | TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.Default).Unwrap(); 
            
            if (!TryGetQueueItem(out var entry))
            {
                return awaitSemaphoreTask;
            }

            var awaitDateTimeTask = AwaitDateTime(entry.Priority, cancellationToken).ContinueWith(t => entry.Item, cancellationToken,
                    TaskContinuationOptions.DenyChildAttach | TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.Default);

            return Task.WhenAny(awaitDateTimeTask, awaitSemaphoreTask).Unwrap();
        }

        public async Task<TData> DequeueAsync(CancellationToken cancellationToken = default)
        {
            var semaphore = new SemaphoreSlim(1);
            _enqueueSemaphoreList.Add(semaphore);
            try
            {
                if (TryGetQueueItem(out var entry, priority => _priorityComparer.Compare(priority, SystemClock.DateTimeOffsetNow()) < 0)
                    && _priorityQueue.TryRemove(entry.Item))
                {
                    return entry.Item;
                }

                var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

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

        public Task<bool> EnqueueAsync(TData item, DateTimeOffset priority, CancellationToken cancellationToken = default)
        {
            var success = _priorityQueue.EnqueueWithoutDuplicates(item, priority);

            if (success)
            {
                _enqueueSemaphoreList.ToList().ForEach(semaphore => semaphore?.Release());
            }

            return Task.FromResult(success);
        }
    }
}
