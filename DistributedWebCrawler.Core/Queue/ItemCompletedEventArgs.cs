using System;

namespace DistributedWebCrawler.Core.Queue
{
    public class ItemCompletedEventArgs : ComponentEventArgs
    {
        public ItemCompletedEventArgs(Guid id, string componentName, object result) : base(componentName, result)
        {
            Id = id;
        }

        public Guid Id { get; }
    }

    public class ItemCompletedEventArgs<TResult> : ItemCompletedEventArgs
        where TResult : notnull
    {
        public ItemCompletedEventArgs(Guid id, string componentName, TResult result) : base(id, componentName, result)
        {
        }

        public new TResult Result => (TResult)base.Result;
    }
}