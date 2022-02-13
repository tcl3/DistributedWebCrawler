using AutoFixture.Xunit2;
using DistributedWebCrawler.Core.Interfaces;
using DistributedWebCrawler.Core.Models;
using DistributedWebCrawler.Core.Tests.Attributes;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace DistributedWebCrawler.Core.Tests
{
    public class ComponentFilterTests
    {
        [Theory]
        [MoqAutoData(configureMembers: true)]
        public void MatchAllFilterShouldMatchAllComponents(
            [Frozen] IEnumerable<ICrawlerComponent> crawlerComponents)
        {
            var componentFilter = ComponentFilter.MatchAll;
            foreach (var matchingComponent in crawlerComponents.Skip(1))
            {
                Assert.True(componentFilter.Matches(matchingComponent));
            }
        }

        [Theory]
        [MoqAutoData(configureMembers: true)]
        public void ComponentFilterFromComponentIdShouldMatchComponentsWithGivenId(
            [Frozen] IEnumerable<ICrawlerComponent> crawlerComponents)
        {
            var componentToMatch = crawlerComponents.First();

            var componentIdToMatch = componentToMatch.ComponentInfo.ComponentId;

            var componentFilter = ComponentFilter.FromComponentId(componentIdToMatch);

            Assert.True(componentFilter.Matches(componentToMatch));

            foreach (var nonMatchingComponent in crawlerComponents.Skip(1))
            {
                Assert.False(componentFilter.Matches(nonMatchingComponent));
            }
        }

        [Theory]
        [MoqAutoData(configureMembers: true)]
        public void ComponentFilterFromComponentNameShouldMatchComponentsWithGivenName(
            [Frozen] IEnumerable<ICrawlerComponent> crawlerComponents)
        {
            var componentToMatch = crawlerComponents.First();

            var componentNameToMatch = componentToMatch.ComponentInfo.ComponentName;

            var componentFilter = ComponentFilter.FromComponentName(componentNameToMatch);

            Assert.True(componentFilter.Matches(componentToMatch));

            foreach (var nonMatchingComponent in crawlerComponents.Skip(1))
            {
                Assert.False(componentFilter.Matches(nonMatchingComponent));
            }
        }

        [Theory]
        [MoqAutoData(configureMembers: true)]
        public void ComponentFilterFromComponentIdsShouldMatchAllComponentsWithGivenIds(
            [Frozen] IEnumerable<ICrawlerComponent> crawlerComponents)
        {
            var componentsToMatch = crawlerComponents.Take(2);

            var componentIdsToMatch = componentsToMatch.Select(x => x.ComponentInfo.ComponentId);

            var componentFilter = ComponentFilter.FromComponentIds(componentIdsToMatch);

            foreach (var matchingComponent in componentsToMatch) 
            { 
                Assert.True(componentFilter.Matches(matchingComponent));
            }

            foreach (var nonMatchingComponent in crawlerComponents.Skip(2))
            {
                Assert.False(componentFilter.Matches(nonMatchingComponent));
            }
        }

        [Theory]
        [MoqAutoData(configureMembers: true)]
        public void ComponentFilterFromComponentNamesShouldMatchAllComponentsWithGivenNames(
            [Frozen] IEnumerable<ICrawlerComponent> crawlerComponents)
        {
            var componentsToMatch = crawlerComponents.Take(2);

            var componentNamesToMatch = componentsToMatch.Select(x => x.ComponentInfo.ComponentName);

            var componentFilter = ComponentFilter.FromComponentNames(componentNamesToMatch);

            foreach (var matchingComponent in componentsToMatch)
            {
                Assert.True(componentFilter.Matches(matchingComponent));
            }

            foreach (var nonMatchingComponent in crawlerComponents.Skip(2))
            {
                Assert.False(componentFilter.Matches(nonMatchingComponent));
            }
        }
    }
}
