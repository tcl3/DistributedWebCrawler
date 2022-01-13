using DistributedWebCrawler.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace DistributedWebCrawler.Core
{
    public class ComponentNameProvider : IComponentNameProvider
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
}