using System;
using System.Threading.Tasks;

namespace DistributedWebCrawler.Core.Queue
{
    public class ItemCompletedEventArgs : EventArgs
    {
        public ItemCompletedEventArgs(Guid id, object result)
        {
            Id = id;
            Result = result;
        }

        public Guid Id { get; }
        public object Result { get; init; }
    }

    public class ItemCompletedEventArgs<TResult> : EventArgs
    {
        public ItemCompletedEventArgs(Guid id, TResult result)
        {
            Id = id;
            Result = result;
        }

        public Guid Id { get; }
        public TResult Result { get; init; }
    }
}