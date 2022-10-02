//using DistributedWebCrawler.Core.Interfaces;
//using StackExchange.Redis;
//using System.Diagnostics.CodeAnalysis;
//using System.Runtime.Serialization;

//namespace DistributedWebCrawler.Extensions.Redis
//{


//    public class RedisDateTimePriorityQueue<TData> : IAsyncPriorityQueue<TData, DateTimeOffset>
//    {
//        private readonly HashSet<SemaphoreSlim> _enqueueSemaphoreList;
//        private readonly IConnectionMultiplexerPool _connectionMultiplexerPool;
//        private readonly ISerializer _serializer;

//        private const string QueueName = "test";

//        public RedisDateTimePriorityQueue(IConnectionMultiplexerPool connectionMultiplexerPool, ISerializer serializer)
//        {
//            _enqueueSemaphoreList = new();
//            _connectionMultiplexerPool = connectionMultiplexerPool;
//            _serializer = serializer;
//        }

//        private static async Task AwaitDateTime(DateTimeOffset priority, CancellationToken cancellationToken)
//        {
//            var delay = priority - DateTimeOffset.Now;
//            if (delay > TimeSpan.Zero)
//            {
//                await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
//            }
//        }

//        private async Task<TData?> GetQueueItem(Predicate<DateTimeOffset>? priorityPredicate = null)
//        {
//            var database = await _connectionMultiplexerPool.GetDatabaseAsync().ConfigureAwait(false);
//            var range = await database.SortedSetRangeByRankWithScoresAsync(QueueName, 0, 0, Order.Ascending).ConfigureAwait(false);
//            if (range.Any())
//            {
//                var value = range.First().Element;
//                var result = _serializer.Deserialize<TData>(value.ToString());

//                if (result is null)
//                {
//                    throw new SerializationException("Failed to deserialize item from redis priority queue");
//                }

//                return result;
//            }

//            return default;
//        }

//        private Task<TData> GetQueueItemAndAwait(SemaphoreSlim semaphore, CancellationToken cancellationToken = default)
//        {
//            var awaitSemaphoreTask = semaphore.WaitAsync(cancellationToken)
//                    .ContinueWith(t => GetQueueItemAndAwait(semaphore, cancellationToken), cancellationToken,
//                    TaskContinuationOptions.DenyChildAttach | TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.Default).Unwrap();

//            var entry = GetQueueItem();

//            if (entry is null)
//            {
//                return awaitSemaphoreTask;
//            }

//            var awaitDateTimeTask = AwaitDateTime(entry.Priority, cancellationToken).ContinueWith(t => entry.Item, cancellationToken,
//                    TaskContinuationOptions.DenyChildAttach | TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.Default);

//            return Task.WhenAny(awaitDateTimeTask, awaitSemaphoreTask).Unwrap();
//        }


//        public async Task<TData> DequeueAsync(CancellationToken cancellationToken = default)
//        {
//            var semaphore = new SemaphoreSlim(1);
//            _enqueueSemaphoreList.Add(semaphore);
//            try
//            {
//                var entry = GetQueueItem(priority => _priorityComparer.Compare(priority, DateTimeOffset.Now) < 0)
//                if (entry != null && _priorityQueue.TryRemove(entry.Item))
//                {
//                    return entry.Item;
//                }

//                var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

//                TData item;
//                try
//                {
//                    do
//                    {
//                        item = await GetQueueItemAndAwait(semaphore, cts.Token).ConfigureAwait(false);
//                    } while (!_priorityQueue.TryRemove(item));
//                }
//                finally
//                {
//                    cts.Cancel();
//                }


//                return item;
//            }
//            finally
//            {
//                _enqueueSemaphoreList.Remove(semaphore);
//            }
//        }

//        public async Task<bool> EnqueueAsync(TData item, DateTimeOffset priority, CancellationToken cancellationToken)
//        {
//            var database = await _connectionMultiplexerPool.GetDatabaseAsync().ConfigureAwait(false);

//            var serializedItem = _serializer.Serialize(item);

//            var success = await database.SortedSetAddAsync(QueueName, serializedItem, priority.ToUnixTimeMilliseconds()).ConfigureAwait(false);

//            if (success)
//            {
//                _enqueueSemaphoreList.ToList().ForEach(semaphore => semaphore?.Release());
//            }

//            return success;
//        }
//    }
//}
