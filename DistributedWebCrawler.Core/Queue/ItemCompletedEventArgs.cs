using System;
using System.Threading.Tasks;

namespace DistributedWebCrawler.Core.Queue
{
    public class ItemCompletedEventArgs : ItemCompletedEventArgs<bool>
    {
        public ItemCompletedEventArgs(Guid id, TaskStatus status) : base(id, status)
        {
        }
    }

    public class ItemCompletedEventArgs<TResult> : EventArgs
    {
        public ItemCompletedEventArgs(Guid id, TaskStatus status)
        {
            Id = id;
            Status = status;
        }

        public Guid Id { get; }
        public TaskStatus Status { get; }
        public TResult? Result { get; init; }
    }
}