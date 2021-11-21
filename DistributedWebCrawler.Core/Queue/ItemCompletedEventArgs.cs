using System;

namespace DistributedWebCrawler.Core.Queue
{
    public class ItemCompletedEventArgs : EventArgs
    {
        public ItemCompletedEventArgs(Guid id)
        {
            Id = id;
        }

        public Guid Id { get; }
    }
}
