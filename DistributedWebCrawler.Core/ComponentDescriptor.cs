using DistributedWebCrawler.Core.Attributes;
using DistributedWebCrawler.Core.Models;
using DistributedWebCrawler.Core.Extensions;
using DistributedWebCrawler.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

namespace DistributedWebCrawler.Core
{
    public class ComponentDescriptor
    {
        public Type ComponentType { get; }
        public Type SuccessType { get; }
        public Type FailureType { get; }
        public string ComponentName { get; }

        internal ComponentDescriptor(Type componentType, Type successType, Type failureType, string componentName)
        {
            ComponentType = componentType;
            SuccessType = successType;
            FailureType = failureType;
            ComponentName = componentName;
        }

        public static IEnumerable<ComponentDescriptor> FromAssemblies(IEnumerable<Assembly> componentAssemblies)
        {
            var assemblyTypes = componentAssemblies.SelectMany(x => x.ExportedTypes);
            var componentDescriptors = new List<ComponentDescriptor>();
            foreach (var type in assemblyTypes)
            {
                var interfaces = type.GetInterfaces();
                
                if (!interfaces.Any(x => x.IsGenericType
                        && x.GetGenericTypeDefinition() == typeof(IRequestProcessor<>)))
                {
                    continue;
                }

                var componentAttribute = type.GetCustomAttribute<ComponentAttribute>();
                if (componentAttribute == null)
                {
                    continue;
                }

                componentDescriptors.Add(new ComponentDescriptor(type, componentAttribute.SuccessType, componentAttribute.FailureType, componentAttribute.ComponentName));
            }            

            return componentDescriptors;
        }
    }
}
