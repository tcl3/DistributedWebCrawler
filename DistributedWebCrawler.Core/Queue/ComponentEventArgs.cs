using System;

namespace DistributedWebCrawler.Core.Queue
{
    public class ComponentEventArgs : EventArgs
    {
        public ComponentEventArgs(string componentName, object result)
        {
            ComponentName = componentName;
            Result = result;
        }

        public string ComponentName { get; }
        public object Result { get; }
    }

    public class ComponentEventArgs<TResult> : ComponentEventArgs
        where TResult : notnull
    {
        public ComponentEventArgs(string componentName, TResult result) : base(componentName, result)
        {
        }

        public new TResult Result => (TResult)base.Result;
    }
}
