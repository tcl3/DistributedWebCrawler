using AutoFixture.Xunit2;
using DistributedWebCrawler.Core.Interfaces;
using DistributedWebCrawler.Core.Models;
using DistributedWebCrawler.Core.Tests.Attributes;
using DistributedWebCrawler.Core.Tests.Fakes;
using Moq;
using Xunit;

namespace DistributedWebCrawler.Core.Tests
{
    public class EventReceiverCollectionTests
    {
        [Theory]
        [MoqAutoData]
        public void SchedulerPropertyShouldCallCorrectEventReceiver(
            [Frozen] Mock<IEventReceiverFactory> eventReceiverFactoryMock,
            EventReceiverCollection sut)
        {
            var result = sut.Scheduler;
            
            Assert.NotNull(result);
            eventReceiverFactoryMock.Verify(x => 
                x.Get<SchedulerSuccess, ErrorCode<SchedulerFailure>>(), Times.Once());
        }

        [Theory]
        [MoqAutoData]
        public void IngesterPropertyShouldCallCorrectEventReceiver(
            [Frozen] Mock<IEventReceiverFactory> eventReceiverFactoryMock,
            EventReceiverCollection sut)
        {
            var result = sut.Ingester;

            Assert.NotNull(result);
            eventReceiverFactoryMock.Verify(x =>
                x.Get<IngestSuccess, IngestFailure>(), Times.Once());
        }

        [Theory]
        [MoqAutoData]
        public void ParserPropertyShouldCallCorrectEventReceiver(
            [Frozen] Mock<IEventReceiverFactory> eventReceiverFactoryMock,
            EventReceiverCollection sut)
        {
            var result = sut.Parser;

            Assert.NotNull(result);
            eventReceiverFactoryMock.Verify(x =>
                x.Get<ParseSuccess, ErrorCode<ParseFailure>>(), Times.Once());
        }

        [Theory]
        [MoqAutoData]
        public void RobotsDownloaderPropertyShouldCallCorrectEventReceiver(
            [Frozen] Mock<IEventReceiverFactory> eventReceiverFactoryMock,
            EventReceiverCollection sut)
        {
            var result = sut.RobotsDownloader;

            Assert.NotNull(result);
            eventReceiverFactoryMock.Verify(x =>
                x.Get<RobotsDownloaderSuccess, ErrorCode<RobotsDownloaderFailure>>(), Times.Once());
        }

        [Theory]
        [MoqAutoData]
        public void OfTypeShouldCallCorrectEventReceiver(
            [Frozen] Mock<IEventReceiverFactory> eventReceiverFactoryMock,
            EventReceiverCollection sut)
        {
            var result = sut.OfType<TestSuccess, ErrorCode<TestFailure>>();

            Assert.NotNull(result);
            eventReceiverFactoryMock.Verify(x =>
                x.Get<TestSuccess, ErrorCode<TestFailure>>(), Times.Once());
        }

        [Theory]
        [MoqAutoData]
        public void GetEnumeratorShouldReturnAllEventReceivers(
            [Frozen] IEventReceiverFactory eventReceiverFactory,
            EventReceiverCollection sut)
        {
            var result = sut.GetEnumerator();

            Assert.NotNull(result);
            Assert.Equal(eventReceiverFactory.GetAll().GetEnumerator(), result);
        }
    }
}
