using AutoFixture.Xunit2;
using DistributedWebCrawler.Core.Interfaces;
using DistributedWebCrawler.Core.Seeding;
using DistributedWebCrawler.Core.Tests.Attributes;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace DistributedWebCrawler.Core.Tests
{
    public class CompositeSeederTests
    {
        [Theory]
        [MoqAutoData]
        public async Task EnsureAllUnderlyingSeederComponentsAreCalled(
            [Frozen] IEnumerable<ISeederComponent> seederComponents,
            CompositeSeeder sut)
        {
            await sut.SeedAsync();

            foreach (var seederComponent in seederComponents)
            {
                var seederComponentMock = Mock.Get(seederComponent);
                seederComponentMock.Verify(x => x.SeedAsync(), Times.Once());
            }
        }
    }
}
