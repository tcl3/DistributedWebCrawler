using System;

namespace DistributedWebCrawler.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    internal class ComponentNameAttribute : Attribute
    {
        public string ComponentName { get; }

        public ComponentNameAttribute(string name)
        {
            ComponentName = name;
        }
    }
}
