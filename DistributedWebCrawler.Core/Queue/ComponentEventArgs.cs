using DistributedWebCrawler.Core.Models;
using System;

namespace DistributedWebCrawler.Core.Queue
{
    public class ComponentEventArgs : EventArgs
    {
        public ComponentEventArgs(NodeInfo nodeInfo, object result)
        {
            NodeInfo = nodeInfo;
            Result = result;
        }

        public NodeInfo NodeInfo { get; }
        public object Result { get; }
    }

    public class ComponentEventArgs<TResult> : EventArgs
        where TResult : notnull
    {
        public ComponentEventArgs(NodeInfo nodeInfo, TResult result)
        {
            NodeInfo = nodeInfo;
            Result = result;
        }

        public NodeInfo NodeInfo { get; }

        public TResult Result { get; }
    }
}
