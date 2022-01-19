using DistributedWebCrawler.Core.Queue;
using DistributedWebCrawler.Core.Tests.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace DistributedWebCrawler.Core.Tests
{
    [Collection(nameof(SystemClockDependentCollection))]
    public class InMemoryDateTimePriorityQueueTests
    {
        private readonly CancellationTokenSource _cts = new(TimeSpan.FromSeconds(1));

        private const int NumberOfItemsToAdd = 100;

        [Fact]
        public async Task DequeueShouldReturnEnqueuedItem()
        {
            var sut = new InMemoryDateTimePriorityQueue<int>();

            var itemToEnqueue = 1;

            var enqueueSuccess = await sut.EnqueueAsync(itemToEnqueue, DateTimeOffset.Now, _cts.Token);
            Assert.True(enqueueSuccess);
            
            var dequeuedItem = await sut.DequeueAsync(_cts.Token);            
            Assert.Equal(itemToEnqueue, dequeuedItem);
        }

        [Fact]
        public async Task DequeuedItemsShouldBeInPriorityOrder()
        {
            var startTime = DateTimeOffset.Now;
            var sut = new InMemoryDateTimePriorityQueue<int>();

            var itemsToEnqueue = Enumerable.Range(0, NumberOfItemsToAdd);
            foreach (var itemToEnqueue in itemsToEnqueue)
            {
                var enqueueSuccess = await sut.EnqueueAsync(itemToEnqueue, startTime.AddMilliseconds(-itemToEnqueue), _cts.Token);
                Assert.True(enqueueSuccess);
            }            

            var dequeueTasks = new List<Task<int>>();
            for (int i = 0; i < itemsToEnqueue.Count(); i++) 
            { 
                dequeueTasks.Add(sut.DequeueAsync(_cts.Token));
            }

            var results = await Task.WhenAll(dequeueTasks);

            Assert.Equal(itemsToEnqueue.Reverse(), results);
        }

        [Fact]
        public async Task DequeueBeforeEnqueueShouldReturnEnqueuedItems()
        {
            var startTime = DateTimeOffset.Now;
            var sut = new InMemoryDateTimePriorityQueue<int>();

            var itemsToEnqueue = Enumerable.Range(0, NumberOfItemsToAdd);

            var dequeueTasks = new List<Task<int>>();
            for (int i = 0; i < itemsToEnqueue.Count(); i++)
            {
                dequeueTasks.Add(sut.DequeueAsync(_cts.Token));
            }
            
            foreach (var itemToEnqueue in itemsToEnqueue)
            {
                var enqueueSuccess = await sut.EnqueueAsync(itemToEnqueue, startTime.AddMilliseconds(-itemToEnqueue), _cts.Token);
                Assert.True(enqueueSuccess);
            }

            // The order of the results is not well defined here
            var results = await Task.WhenAll(dequeueTasks);

            Assert.Equal(itemsToEnqueue, results.OrderBy(x => x));
        }



        [Fact]
        public async Task DequeueShouldNotReturnUntilEnqueueDateIsReached()
        {
            var fixedTime = DateTimeOffset.Now;
            var enqueuePriority = fixedTime.AddMilliseconds(1);
            try
            {
                SystemClock.DateTimeOffsetNow = () => fixedTime;
                SystemClock.DelayAsync = (timespan, _) =>
                {
                    Assert.Equal(timespan, enqueuePriority - fixedTime);
                    return Task.CompletedTask;
                };

                var sut = new InMemoryDateTimePriorityQueue<int>();

                var itemToEnqueue = 1;
                await sut.EnqueueAsync(itemToEnqueue, enqueuePriority, _cts.Token);

                var dequeuedItem = await sut.DequeueAsync(_cts.Token);

                Assert.Equal(itemToEnqueue, dequeuedItem);
            } 
            finally
            {
                SystemClock.Reset();
            }
        }
    }
}
