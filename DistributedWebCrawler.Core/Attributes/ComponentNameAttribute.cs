﻿using System;

namespace DistributedWebCrawler.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    internal class ComponentNameAttribute : Attribute
    {
        public string ComponentName { get; }
        public Type SuccessType { get; }
        public Type FailureType { get; }

        // TODO: replace the type properties here with generics when the Attribute generics feature comes out of preview
        public ComponentNameAttribute(string name, Type successType, Type failureType)
        {
            ComponentName = name;
            SuccessType = successType;
            FailureType = failureType;
        }
    }
}
