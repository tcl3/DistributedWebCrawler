using DistributedWebCrawler.Core.Interfaces;
using DistributedWebCrawler.Core.Models;
using System;

namespace DistributedWebCrawler.Core.Queue
{
    public class ItemCompletedEventArgs : ComponentEventArgs
    {
        public ItemCompletedEventArgs(Guid id, ComponentInfo nodeInfo, object result) : base(nodeInfo, result)
        {
            Id = id;
        }

        public Guid Id { get; }
    }

    public class ItemCompletedEventArgs<TResult> : ItemCompletedEventArgs
        where TResult : notnull
    {
        public ItemCompletedEventArgs(Guid id, ComponentInfo nodeInfo, TResult result) : base(id, nodeInfo, result)
        {
         
        }

        public new TResult Result => (TResult)base.Result;
    }

    public class ItemFailedEventArgs : ComponentEventArgs<IErrorCode>
    {
        public ItemFailedEventArgs(Guid id, ComponentInfo nodeInfo, IErrorCode result) : base(nodeInfo, result)
        {
            Id = id;
        }

        public Guid Id { get; }
    }

    public class ItemFailedEventArgs<TFailure> : ItemFailedEventArgs
        where TFailure : notnull, IErrorCode
    {
        public ItemFailedEventArgs(Guid id, ComponentInfo nodeInfo, TFailure result) : base(id, nodeInfo, result)
        {
        }

        public new TFailure Result => (TFailure)base.Result;
    }
}