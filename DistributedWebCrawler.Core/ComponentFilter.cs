using DistributedWebCrawler.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DistributedWebCrawler.Core
{
    public class ComponentFilter
    {        
        public IEnumerable<string> ComponentNames { get; }
        public IEnumerable<Guid> ComponentIds { get; }

        private static readonly ComponentFilter _matchAllFilter = new(Enumerable.Empty<string>(), Enumerable.Empty<Guid>());
        public static ComponentFilter MatchAll => _matchAllFilter;

        public ComponentFilter(IEnumerable<string> componentNames, IEnumerable<Guid> componentIds)
        {
            ComponentNames = componentNames;
            ComponentIds = componentIds;
        }

        public static ComponentFilter FromComponentNames(IEnumerable<string> componentNames)
        {
            return new ComponentFilter(componentNames, Enumerable.Empty<Guid>());
        }

        public static ComponentFilter FromComponentName(string componentName)
        {
            return new ComponentFilter(new[] { componentName }, Enumerable.Empty<Guid>());
        }

        public static ComponentFilter FromComponentIds(IEnumerable<Guid> componentIds)
        {
            return new ComponentFilter(Enumerable.Empty<string>(), componentIds);
        }

        public static ComponentFilter FromComponentId(Guid componentId)
        {
            return new ComponentFilter(Enumerable.Empty<string>(), new[] { componentId });
        }

        public bool Matches(ICrawlerComponent component)
        {
            if (ComponentIds.Any() && !ComponentIds.Contains(component.ComponentInfo.ComponentId)) 
            {
                return false;
            }

            if (ComponentNames.Any() && !ComponentNames.Contains(component.ComponentInfo.ComponentName))
            {
                return false;
            }

            return true;
        }
    }
}
