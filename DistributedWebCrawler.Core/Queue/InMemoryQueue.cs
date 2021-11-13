using DistributedWebCrawler.Core.Interfaces;
using Priority_Queue;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace DistributedWebCrawler.Core.Queue
{
    public class InMemoryQueue<TData> : IProducerConsumer<TData>
    {
        private readonly ConcurrentQueue<TData> _queue;
        
        public InMemoryQueue()
        {
            _queue = new();
        }

        public int Count => _queue.Count;

        public void Enqueue(TData data)
        {
            _queue.Enqueue(data);
        }

        public bool TryDequeue([NotNullWhen(returnValue: true)] out TData? data)
        {
            var result = _queue.TryDequeue(out data);

            if (result && data == null)
            {
                return false;
            }

            return result;
        }
    }
}
