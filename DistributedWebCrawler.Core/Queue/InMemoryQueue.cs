using DistributedWebCrawler.Core.Interfaces;
using DistributedWebCrawler.Core.Models;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace DistributedWebCrawler.Core.Queue
{
    public class InMemoryQueue<TRequest> : IProducerConsumer<TRequest>
        where TRequest : RequestBase
    {
        private readonly ConcurrentQueue<TRequest> _queue;
        private readonly ConcurrentQueue<TaskCompletionSource<TRequest>> _taskQueue;

        public InMemoryQueue()
        {
            _queue = new();
            _taskQueue = new();
        }

        public int Count => _queue.Count;

        public void Enqueue(TRequest data)
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

        public Task<TRequest> DequeueAsync()
        {
            if (_queue.TryDequeue(out var data))
            {
                return Task.FromResult(data);
            }

            var tcs = new TaskCompletionSource<TRequest>();

            _taskQueue.Enqueue(tcs);

            return tcs.Task;            
        }
    }
}
