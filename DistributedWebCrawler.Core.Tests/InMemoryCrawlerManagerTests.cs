using AutoFixture.Xunit2;
using DistributedWebCrawler.Core.Enums;
using DistributedWebCrawler.Core.Interfaces;
using DistributedWebCrawler.Core.Tests.Attributes;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace DistributedWebCrawler.Core.Tests
{
    public class InMemoryCrawlerManagerTests
    {
        [MoqAutoData]
        [Theory]
        public async Task StartShouldCallAllCrawlerComponents(
            [Frozen] IEnumerable<ICrawlerComponent> crawlerComponents,            
            CrawlerRunningState crawlerRunningState,
            InMemoryCrawlerManager sut)
        {
            await sut.StartAsync();

            foreach (var crawlerComponent in crawlerComponents)
            {
                var componentMock = Mock.Get(crawlerComponent);
                componentMock.Verify(x => x.StartAsync(crawlerRunningState, default), Times.Once());
            }
        }

        [MoqAutoData]
        [Theory]
        public async Task CallingStartMultipleTimesShouldThrowException(InMemoryCrawlerManager sut)
        {
            await sut.StartAsync();
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await sut.StartAsync());
        }

        [MoqAutoData]
        [Theory]
        public async Task StartShouldCallSeeder(
            [Frozen] Mock<ISeeder> seederMock,
            InMemoryCrawlerManager sut)
        {
            await sut.StartAsync();
            seederMock.Verify(x => x.SeedAsync(), Times.Once());
        }

        [MoqAutoData]
        [Theory]
        public async Task PauseShouldCallAllCrawlerComponents(
            [Frozen] IEnumerable<ICrawlerComponent> crawlerComponents,
            InMemoryCrawlerManager sut)
        {
            await sut.PauseAsync();

            foreach (var crawlerComponent in crawlerComponents)
            {
                var componentMock = Mock.Get(crawlerComponent);
                componentMock.Verify(x => x.PauseAsync(), Times.Once());
            }
        }

        [MoqAutoData]
        [Theory]
        public async Task ResumeShouldCallAllCrawlerComponents(
            [Frozen] IEnumerable<ICrawlerComponent> crawlerComponents,
            InMemoryCrawlerManager sut)
        {
            await sut.ResumeAsync();

            foreach (var crawlerComponent in crawlerComponents)
            {
                var componentMock = Mock.Get(crawlerComponent);
                componentMock.Verify(x => x.ResumeAsync(), Times.Once());
            }
        }

        [MoqAutoData]
        [Theory]
        public async Task WaitUntilCompletedShouldCallAllCrawlerComponents(
            [Frozen] IEnumerable<ICrawlerComponent> crawlerComponents,
            InMemoryCrawlerManager sut)
        {
            await sut.WaitUntilCompletedAsync();

            foreach (var crawlerComponent in crawlerComponents)
            {
                var componentMock = Mock.Get(crawlerComponent);
                componentMock.Verify(x => x.WaitUntilCompletedAsync(), Times.Once());
            }
        }
    }
}
