using DistributedWebCrawler.Core.Tests.Fakes;
using System.Linq;
using Xunit;

namespace DistributedWebCrawler.Core.Tests
{
    public class ComponentDescriptorTests
    {
        [Fact]
        public void FromAssembliesShouldReturnAllComponentsInAssembly()
        {
            var expectedComponentTypes = new[] { typeof(TestRequestProcessor)};
            var componentDescriptors = ComponentDescriptor.FromAssemblies(new[] { typeof(TestComponentMarkerInterface).Assembly }).ToList();

            Assert.Equal(expectedComponentTypes.Length, componentDescriptors.Count);

            var expectedComponentTypeNames = expectedComponentTypes.Select(x => x.FullName).OrderBy(x => x);
            Assert.Equal(expectedComponentTypeNames, componentDescriptors.Select(x => x.ComponentType.FullName).OrderBy(x => x));
        }
    }
}
