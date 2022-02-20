using DistributedWebCrawler.Core.Queue;
using DistributedWebCrawler.Core.Tests.Fakes;
using System;
using System.Threading.Tasks;
using Xunit;

namespace DistributedWebCrawler.Core.Tests
{
    public class InMemoryQueueTests
    {
        private static readonly TestRequest TestRequestData = new TestRequest(new Uri("http://test.uri/"));

        [Fact]
        public void NewQueueCountShouldBeZero()
        {
            var sut = new InMemoryQueue<TestRequest>();
            Assert.Equal(0, sut.Count);
        }

        [Fact]
        public async Task DequeueShouldReturnEnqueuedItem()
        {
            var sut = new InMemoryQueue<TestRequest>();
            
            sut.Enqueue(TestRequestData);
            Assert.Equal(1, sut.Count);

            var dequeuedItem = await sut.DequeueAsync();
            Assert.Equal(TestRequestData, dequeuedItem);
            Assert.Equal(0, sut.Count);
        }

        [Fact]
        public async Task DequeueBeforeEnqueueShouldReturnEnqueuedItem()
        {
            var sut = new InMemoryQueue<TestRequest>();

            var dequeueTask = sut.DequeueAsync();

            sut.Enqueue(TestRequestData);

            var dequeuedItem = await dequeueTask;
            
            Assert.Equal(TestRequestData, dequeuedItem);
            Assert.Equal(0, sut.Count);
        }
    }
}
