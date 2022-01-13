using DistributedWebCrawler.Core.Components;
using DistributedWebCrawler.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

namespace DistributedWebCrawler.Core
{
    public class ComponentNameProvider
    {
        private readonly IEnumerable<ComponentDescriptor> _descriptors;

        private readonly IReadOnlyDictionary<Type, ComponentDescriptor> _descriptorComponentTypeLookup;
        private readonly IReadOnlyDictionary<Type, ComponentDescriptor> _descriptorSuccessTypeLookup;
        private readonly IReadOnlyDictionary<Type, ComponentDescriptor> _descriptorFailureTypeLookup;

        public ComponentNameProvider(IEnumerable<ComponentDescriptor> descriptors)
        {
            _descriptors = descriptors;
            _descriptorComponentTypeLookup = ConstructTypeLookup(descriptors, descriptor => descriptor.ComponentType);
            _descriptorSuccessTypeLookup = ConstructTypeLookup(descriptors, descriptor => descriptor.SuccessType);
            _descriptorFailureTypeLookup = ConstructTypeLookup(descriptors, descriptor => descriptor.FailureType);
        }


        private static IReadOnlyDictionary<Type, ComponentDescriptor> ConstructTypeLookup(IEnumerable<ComponentDescriptor> descriptors, Func<ComponentDescriptor, Type> typeGetterFunc)
        {
            var dictionary = new Dictionary<Type, ComponentDescriptor>();
            foreach (var descriptor in descriptors)
            {
                var type = typeGetterFunc(descriptor);
                AddTypeToLookup(dictionary, descriptor, type);
            }

            return dictionary;
        }

        private static void AddTypeToLookup(Dictionary<Type, ComponentDescriptor> lookup, ComponentDescriptor descriptor, Type type)
        {
            if (!lookup.TryAdd(type, descriptor))
            {
                throw new InvalidOperationException($"Duplicate type {type.Name} found when constructing {nameof(ComponentNameProvider)}");
            }
        }

        public bool TryGetFromComponentType(Type componentType, [NotNullWhen(returnValue: true)] out ComponentDescriptor? descriptor)
        {
            if (!_descriptorComponentTypeLookup.TryGetValue(componentType, out descriptor))
            {
                return false;
            }

            return true;
        }

        public bool TryGetFromTypeArguments(Type successType, Type failureType, [NotNullWhen(returnValue: true)] out ComponentDescriptor? descriptor)
        {
            if (_descriptorSuccessTypeLookup.TryGetValue(successType, out var successTypeDescriptor)
                && _descriptorFailureTypeLookup.TryGetValue(failureType, out var failureTypeDescriptor)
                && successTypeDescriptor == failureTypeDescriptor)
            {
                descriptor = successTypeDescriptor;
                return true;
            }

            descriptor = null;
            return false;
        }
    }

    public static class ComponentNameProviderExtensions
    {
        private const string DefaultComponentName = "Unknown";
        public static ComponentDescriptor GetFromComponentType(this ComponentNameProvider componentNameProvider, Type componentType)
        {
            if (!componentNameProvider.TryGetFromComponentType(componentType, out var result))
            {
                throw new KeyNotFoundException($"Component descriptor for type {componentType.Name} not found");
            }

            return result;
        }

        public static ComponentDescriptor GetFromComponentType<TData>(this ComponentNameProvider componentNameProvider)
        {
            return componentNameProvider.GetFromComponentType(typeof(TData));
        }

        public static string GetComponentName(this ComponentNameProvider componentNameProvider, Type componentType)
        {
            var descriptor = componentNameProvider.GetFromComponentType(componentType);
            return descriptor.ComponentName;
        }

        public static string GetComponentName<TComponent>(this ComponentNameProvider componentNameProvider)
        {
            return componentNameProvider.GetComponentName(typeof(TComponent));
        }

        public static string GetComponentNameOrDefault(this ComponentNameProvider componentNameProvider, Type componentType, string defaultValue = DefaultComponentName)
        {
            return componentNameProvider.GetComponentNameOrDefault(componentType, () => defaultValue);
        }

        public static string GetComponentNameOrDefault(this ComponentNameProvider componentNameProvider, Type componentType, Func<string> defaultValueFactory)
        {
            if (componentNameProvider.TryGetFromComponentType(componentType, out var descriptor))
            {
                return descriptor.ComponentName;
            }

            return defaultValueFactory();
        }

        public static string GetComponentNameOrDefault<TComponent>(this ComponentNameProvider componentNameProvider, string defaultValue = DefaultComponentName)
        {
            return componentNameProvider.GetComponentNameOrDefault(typeof(TComponent), defaultValue);
        }

        public static string GetComponentNameOrDefault<TComponent>(this ComponentNameProvider componentNameProvider, Func<string> defaultValueFactory)
        {
            return componentNameProvider.GetComponentNameOrDefault(typeof(TComponent), defaultValueFactory);
        }

        public static ComponentDescriptor GetFromTypeArguments(this ComponentNameProvider componentNameProvider, Type successType, Type failureType)
        {
            if (!componentNameProvider.TryGetFromTypeArguments(successType, failureType, out var result))
            {
                throw new KeyNotFoundException($"Component descriptor for type arguments {successType.Name} and {failureType.Name} not found");
            }

            return result;
        }

        public static string GetComponentName(this ComponentNameProvider componentNameProvider, Type successType, Type failureType)
        {
            var descriptor = componentNameProvider.GetFromTypeArguments(successType, failureType);
            return descriptor.ComponentName;
        }

        public static string GetComponentNameOrDefault(this ComponentNameProvider componentNameProvider, Type successType, Type failureType, string defaultValue = DefaultComponentName)
        {
            return componentNameProvider.GetComponentNameOrDefault(successType, failureType, () => defaultValue);
        }

        public static string GetComponentNameOrDefault(this ComponentNameProvider componentNameProvider, Type successType, Type failureType, Func<string> defaultValueFactory)
        {
            if (componentNameProvider.TryGetFromTypeArguments(successType, failureType, out var descriptor))
            {
                return descriptor.ComponentName;
            }

            return defaultValueFactory();
        }

        public static string GetComponentNameOrDefault<TSuccess, TFailure>(this ComponentNameProvider componentNameProvider, string defaultValue = DefaultComponentName)
        {
            return componentNameProvider.GetComponentNameOrDefault(typeof(TSuccess), typeof(TFailure), () => defaultValue);
        }

        public static string GetComponentNameOrDefault<TSuccess, TFailure>(this ComponentNameProvider componentNameProvider, Func<string> defaultValueFactory)
        {
            return componentNameProvider.GetComponentNameOrDefault(typeof(TSuccess), typeof(TFailure), defaultValueFactory);
        }
    }
}