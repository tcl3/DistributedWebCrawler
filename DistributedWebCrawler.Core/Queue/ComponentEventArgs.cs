using DistributedWebCrawler.Core.Models;
using System;

namespace DistributedWebCrawler.Core.Queue
{
    public class ComponentEventArgs : EventArgs
    {
        public ComponentEventArgs(ComponentInfo nodeInfo, object result)
        {
            ComponentInfo = nodeInfo;
            Result = result;
        }

        public ComponentInfo ComponentInfo { get; }
        public object Result { get; }
    }

    public class ComponentEventArgs<TResult> : EventArgs
        where TResult : notnull
    {
        public ComponentEventArgs(ComponentInfo componentInfo, TResult result)
        {
            ComponentInfo = componentInfo;
            Result = result;
        }

        public ComponentInfo ComponentInfo { get; }

        public TResult Result { get; }
    }
}
