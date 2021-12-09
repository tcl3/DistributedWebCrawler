using DistributedWebCrawler.Core.Extensions;
using System;
using System.Diagnostics.CodeAnalysis;

namespace DistributedWebCrawler.Core
{
    public static class ComponentNameProviderExtensions
    {
        public static string GetComponentNameOrDefault<TSuccess, TFailure>(
            this ComponentNameProvider<TSuccess, TFailure> provider, Func<string> defaultValueFactory)
        {
            return provider.TryGetComponentName(out var componentName) 
                ? componentName 
                : defaultValueFactory();
        }

        public static string GetComponentNameOrDefault<TSuccess, TFailure>(
            this ComponentNameProvider<TSuccess, TFailure> provider, string defaultValue = "Unknown")
        {
            return provider.TryGetComponentName(out var componentName)
                ? componentName
                : defaultValue;
        }
    }
    public class ComponentNameProvider<TSuccess, TFailure>
    {
        private static readonly string ComponentName = string.Empty;

        static ComponentNameProvider()
        {
            var commonPrefix = typeof(TSuccess).Name.GetCommonPrefix(typeof(TFailure).Name);
            if (!string.IsNullOrEmpty(commonPrefix))
            {
                var componentName = commonPrefix;
                if (componentName.EndsWith('e'))
                {
                    componentName += 'r';
                }
                else if (!componentName.EndsWith("er"))
                {

                    componentName += "er";
                }
                ComponentName = componentName;
            }
        }

        public bool TryGetComponentName([NotNullWhen(returnValue: true)] out string componentName)
        {
            componentName = ComponentName;
            return componentName != string.Empty;
        }
    }
}
