using AutoFixture.Xunit2;
using DistributedWebCrawler.Core.Components;
using DistributedWebCrawler.Core.Interfaces;
using DistributedWebCrawler.Core.Model;
using DistributedWebCrawler.Core.Queue;
using DistributedWebCrawler.Core.Tests.Attributes;
using DistributedWebCrawler.Core.Tests.Fakes;
using System.Threading.Tasks;
using Xunit;

namespace DistributedWebCrawler.Core.Tests
{
    public class InMemoryEventDispatcherTests
        : InMemoryEventDispatcherTests<TestRequest, TestSuccess, ErrorCode<TestFailure>>
    {

    }

    public abstract class InMemoryEventDispatcherTests<TRequest, TSuccess, TFailure>
        where TRequest : RequestBase
        where TSuccess : notnull
        where TFailure : notnull, IErrorCode
    {
        [MoqAutoData]
        [Theory]
        public async Task EnsureEventFiresWhenNotifyCompletedIsCalled(
            [Frozen] InMemoryEventStore<TSuccess, TFailure> eventStore,
            InMemoryEventDispatcher<TSuccess, TFailure> sut, 
            TestRequest request, 
            TSuccess result)
        {
            var eventCalled = false;
            eventStore.OnCompletedAsyncHandler += (sender, e) =>
            {
                Assert.NotNull(sender);
                Assert.Equal(result, e.Result);
                
                eventCalled = true;

                return Task.CompletedTask;
            };
            await sut.NotifyCompletedAsync(request, result);
            Assert.True(eventCalled);
        }

        [MoqAutoData]
        [Theory]
        public async Task EnsureEventFiresWhenNotifyFailedIsCalled(
            [Frozen] InMemoryEventStore<TSuccess, TFailure> eventStore,
            InMemoryEventDispatcher<TSuccess, TFailure> sut,
            TestRequest request,
            TFailure result)
        {
            var eventCalled = false;
            eventStore.OnFailedAsyncHandler += (sender, e) =>
            {
                Assert.NotNull(sender);
                Assert.Equal(result, e.Result);

                eventCalled = true;

                return Task.CompletedTask;
            };

            await sut.NotifyFailedAsync(request, result);
            Assert.True(eventCalled);
        }

        [MoqAutoData]
        [Theory]
        public async Task EnsureEventFiresWhenNotifyComponentUpdateIsCalled(
            [Frozen] InMemoryEventStore<TSuccess, TFailure> eventStore,
            InMemoryEventDispatcher<TSuccess, TFailure> sut,
            ComponentStatus result)
        {
            var eventCalled = false;
            eventStore.OnComponentUpdateAsyncHandler += (sender, e) =>
            {
                Assert.NotNull(sender);
                Assert.Equal(result, e.Result);

                eventCalled = true;

                return Task.CompletedTask;
            };

            await sut.NotifyComponentStatusUpdateAsync(result);
            Assert.True(eventCalled);
        } 
    }
}
