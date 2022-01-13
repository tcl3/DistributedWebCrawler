using DistributedWebCrawler.Core.Attributes;
using DistributedWebCrawler.Core.Components;
using DistributedWebCrawler.Core.Extensions;
using DistributedWebCrawler.Core.Model;
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
            var components = componentAssemblies
                .SelectMany(x => x.ExportedTypes)
                .Select(x => new { Type = x, BaseTypes = x.GetBaseTypes()})
                .SelectMany(x => x.BaseTypes, (parent, baseType) => new { Type = parent.Type, BaseType = baseType })
                .Where(x =>
                {
                    return x.BaseType.IsGenericType
                            && x.BaseType.GetGenericTypeDefinition() == typeof(AbstractTaskQueueComponent<,,>);
                }
            );

            var descriptors = new List<ComponentDescriptor>();
            foreach (var component in components)
            {
                var genericArgs = component.BaseType.GetGenericArguments();

                if (genericArgs.Length != 3)
                {
                    throw new InvalidOperationException($"Unexpected number of generic arguments found for component type {component.Type}. Expected 3, found {genericArgs.Length}");
                }

                var successType = genericArgs[1];
                var failureType = genericArgs[2];

                if (!TryGetComponentNameFromAttribute(component.Type, out var componentName)
                    && !TryGetComponentNameFromTypeArguments(successType, failureType, out componentName))
                {
                    throw new InvalidOperationException($"Component name for {component.Type.Name} could not be inferred from type arguments: {successType.Name} and {failureType.Name}");
                }

                var descriptor = new ComponentDescriptor(component.Type, successType, failureType, componentName);
                descriptors.Add(descriptor);
            }

            return descriptors;
        }

        private static bool TryGetComponentNameFromAttribute(Type componentType, [NotNullWhen(returnValue: true)] out string? componentName)
        {
            var componentaNameAttribute = componentType.GetCustomAttribute<ComponentNameAttribute>();
            if (componentaNameAttribute != null)
            {
                componentName = componentaNameAttribute.ComponentName;
                return true;
            }

            componentName = null;
            return false;
        }

        private static bool TryGetComponentNameFromTypeArguments(Type successType, Type failureType, out string componentName)
        {
            var failureTypeName = failureType.Name;
            if (failureType.IsGenericType && failureType.GetGenericTypeDefinition() == typeof(ErrorCode<>))
            {
                failureTypeName = failureType.GetGenericArguments()[0].Name;
            }

            componentName = successType.Name.GetCommonPrefix(failureTypeName);
            if (!string.IsNullOrEmpty(componentName))
            {
                if (componentName.EndsWith('e'))
                {
                    componentName += 'r';
                }
                else if (!componentName.EndsWith("er"))
                {
                    componentName += "er";
                }
            }

            return componentName != string.Empty;
        }
    }
}
