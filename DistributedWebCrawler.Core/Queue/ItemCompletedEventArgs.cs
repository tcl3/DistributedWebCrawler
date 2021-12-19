using DistributedWebCrawler.Core.Interfaces;
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

    public class ItemFailedEventArgs : ComponentEventArgs<IErrorCode>
    {
        public ItemFailedEventArgs(Guid id, string componentName, IErrorCode result) : base(componentName, result)
        {
            Id = id;
        }

        public Guid Id { get; }
    }

    public class ItemFailedEventArgs<TFailure> : ItemFailedEventArgs
        where TFailure : notnull, IErrorCode
    {
        public ItemFailedEventArgs(Guid id, string componentName, TFailure result) : base(id, componentName, result)
        {
        }

        public new TFailure Result => (TFailure)base.Result;
    }
}