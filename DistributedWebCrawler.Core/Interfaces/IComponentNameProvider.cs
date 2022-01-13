using System;
using System.Diagnostics.CodeAnalysis;

namespace DistributedWebCrawler.Core.Interfaces
{
    public interface IComponentNameProvider
    {
        bool TryGetFromComponentType(Type componentType, [NotNullWhen(returnValue: true)] out ComponentDescriptor? descriptor);
        bool TryGetFromTypeArguments(Type successType, Type failureType, [NotNullWhen(returnValue: true)] out ComponentDescriptor? descriptor);
    }
}
