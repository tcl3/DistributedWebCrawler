using DistributedWebCrawler.Core.Interfaces;
using DistributedWebCrawler.Core.Model;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace DistributedWebCrawler.Core.Queue
{
    public class InMemoryQueue<TRequest> : InMemoryQueue<TRequest, bool>
        where TRequest : RequestBase
    {
        public InMemoryQueue() : base()
        {

        }
    }

    public class InMemoryQueue<TRequest, TResult> : IProducerConsumer<TRequest, TResult>
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
