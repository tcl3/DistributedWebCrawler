using AutoFixture;
using AutoFixture.Xunit2;
using DistributedWebCrawler.Core.Configuration;
using DistributedWebCrawler.Core.Interfaces;
using DistributedWebCrawler.Core.Models;
using DistributedWebCrawler.Core.Seeding;
using DistributedWebCrawler.Core.Tests.Attributes;
using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace DistributedWebCrawler.Core.Tests
{
    public class SchedulerQueueSeederTests
    {
        [Theory]
        [SeederSettingsAutoData(source: SeederSource.Unknown)]
        public void ConstructorShouldThrowWhenSeederSourceNotSet(IFixture fixture)
        {
            Assert.ThrowsAny<ObjectCreationException>(() => fixture.Create<SchedulerQueueSeeder>());
        }

        [Theory]
        
        [SeederSettingsAutoData(source: SeederSource.ReadFromFile, filePath: "")]
        public void ConstructorShouldThrowWhenReadFromFileSetAndFilePathNotSet(IFixture fixture)
        {
            Assert.ThrowsAny<ObjectCreationException>(() => fixture.Create<SchedulerQueueSeeder>());
        }

        [Theory]
        [SeederSettingsAutoData(source: SeederSource.ReadFromConfig)]
        public void ConstructorShouldThrowWhenNoUrisToCrawl(IFixture fixture)
        {
            Assert.ThrowsAny<ObjectCreationException>(() => fixture.Create<SchedulerQueueSeeder>());
        }

        [Theory]
        [SeederSettingsAutoData(new[] { "http://single.uri/" })]
        [SeederSettingsAutoData(new[] { "http://first.domain/", "http://second.domain/" })]
        public async Task UnderlyingProducerShouldBeCalledForEachUriGiven(
            string[] expectedUris,
            [Frozen] Mock<IProducer<SchedulerRequest>> producer,
            SchedulerQueueSeeder sut)
        {
            var requestList = new List<SchedulerRequest>();
            producer.Setup(x => x.Enqueue(It.IsAny<SchedulerRequest>()))
                .Callback<SchedulerRequest>(request => requestList.Add(request));

            await sut.SeedAsync();

            producer.Verify(x => x.Enqueue(It.IsAny<SchedulerRequest>()), Times.Exactly(expectedUris.Length));

            requestList.Select(x => x.Uri.ToString()).Should().BeEquivalentTo(expectedUris);
        }
    }
}
