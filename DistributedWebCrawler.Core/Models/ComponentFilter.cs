using DistributedWebCrawler.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DistributedWebCrawler.Core.Models
{
    public class ComponentFilter
    {
        public IEnumerable<string> ComponentNames { get; init; } = Enumerable.Empty<string>();
        public IEnumerable<Guid> ComponentIds { get; init; } = Enumerable.Empty<Guid>();

        private static readonly ComponentFilter _matchAllFilter = new();
        public static ComponentFilter MatchAll => _matchAllFilter;

        public static ComponentFilter FromComponentNames(IEnumerable<string> componentNames)
        {
            return new ComponentFilter
            {
                ComponentNames = componentNames,
            };
        }

        public static ComponentFilter FromComponentName(string componentName)
        {
            return new ComponentFilter
            {
                ComponentNames = new[] { componentName } 
            };
        }

        public static ComponentFilter FromComponentIds(IEnumerable<Guid> componentIds)
        {
            return new ComponentFilter
            {
                ComponentIds = componentIds
            };
        }

        public static ComponentFilter FromComponentId(Guid componentId)
        {
            return new ComponentFilter
            {
                ComponentIds = new[] { componentId }
            };
        }

        public bool Matches(ICrawlerComponent component)
        {
            if (ReferenceEquals(this, MatchAll))
            {
                return true;
            }

            if (ComponentIds.Any(id => id == component.ComponentInfo.ComponentId))
            {
                return true;
            }

            if (ComponentNames.Any(name => name.Equals(component.ComponentInfo.ComponentName, StringComparison.InvariantCultureIgnoreCase)))
            {
                return true;
            }

            return false;
        }
    }
}
