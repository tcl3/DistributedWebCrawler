using DistributedWebCrawler.Core.Interfaces;
using Priority_Queue;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace DistributedWebCrawler.Core.Queue
{
    public class InMemoryQueue<TData> : IProducerConsumer<TData>
    {
        private readonly ConcurrentQueue<TData> _queue;
        private readonly ConcurrentQueue<TaskCompletionSource<TData>> _taskQueue;

        public InMemoryQueue()
        {
            _queue = new();
            _taskQueue = new();
        }

        public int Count => _queue.Count;

        public void Enqueue(TData data)
        {
            if (_taskQueue.TryDequeue(out var taskCompletionSource))
            {
                taskCompletionSource.SetResult(data);
            }
            else 
            { 
                _queue.Enqueue(data);
            }
        }

        public Task<TData> DequeueAsync()
        {
            if (_queue.TryDequeue(out var data))
            {
                return Task.FromResult(data);
            }

            var tcs = new TaskCompletionSource<TData>();

            _taskQueue.Enqueue(tcs);

            return tcs.Task;            
        }
    }
}
