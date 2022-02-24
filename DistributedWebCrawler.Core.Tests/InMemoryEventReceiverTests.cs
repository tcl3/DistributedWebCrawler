using AutoFixture.Xunit2;
using DistributedWebCrawler.Core.Models;
using DistributedWebCrawler.Core.Interfaces;
using DistributedWebCrawler.Core.Queue;
using DistributedWebCrawler.Core.Tests.Attributes;
using DistributedWebCrawler.Core.Tests.Fakes;
using System;
using System.Threading.Tasks;
using Xunit;

namespace DistributedWebCrawler.Core.Tests
{
    public class InMemoryEventReceiverTests : InMemoryEventReceiverTests<TestSuccess, ErrorCode<TestFailure>>
    {

    }

    public abstract class InMemoryEventReceiverTests<TSuccess, TFailure>
        where TSuccess : notnull
        where TFailure : notnull, IErrorCode
    {
        [MoqAutoData]
        [Theory]
        public void EnsureOnCompletedEventIsCalled(
            [Frozen] InMemoryEventStore<TSuccess, TFailure> eventStore, 
            InMemoryEventReceiver<TSuccess, TFailure> sut, 
            ItemCompletedEventArgs<TSuccess> args)
        {
            IEventReceiver castedSut = sut;
            var eventCalled = false;

            Task OnCompletedAsync<TArgs>(object? sender, TArgs e)
                where TArgs : EventArgs
            {
                Assert.Equal(this, sender);
                Assert.True(args == e);

                eventCalled = true;
                return Task.CompletedTask;
            }

            var initialDelegateCount = eventStore.OnCompletedAsyncHandler!.GetInvocationList().Length;

            sut.OnCompletedAsync += OnCompletedAsync;
            castedSut.OnCompletedAsync += OnCompletedAsync;

            Assert.NotNull(eventStore.OnCompletedAsyncHandler);

            var x = eventStore.OnCompletedAsyncHandler!.GetInvocationList();
            Assert.Equal(initialDelegateCount + 2, eventStore.OnCompletedAsyncHandler!.GetInvocationList().Length);

            eventStore.OnCompletedAsyncHandler!(this, args);
            Assert.True(eventCalled);

            sut.OnCompletedAsync -= OnCompletedAsync;
            castedSut.OnCompletedAsync -= OnCompletedAsync;
            eventCalled = false;

            Assert.Equal(initialDelegateCount, eventStore.OnCompletedAsyncHandler!.GetInvocationList().Length);

            eventStore.OnCompletedAsyncHandler!(this, args);

            Assert.False(eventCalled);
        }

        [MoqAutoData]
        [Theory]
        public void EnsureOnFailedEventIsCalled([Frozen] InMemoryEventStore<TSuccess, TFailure> eventStore,
            InMemoryEventReceiver<TSuccess, TFailure> sut, ItemFailedEventArgs<TFailure> args)
        {
            IEventReceiver castedSut = sut;
            var eventCalled = false;

            Task OnFailedAsync<TArgs>(object? sender, TArgs e)
                where TArgs : EventArgs
            {
                Assert.Equal(this, sender);
                Assert.True(args == e);

                eventCalled = true;
                return Task.CompletedTask;
            }

            var initialDelegateCount = eventStore.OnFailedAsyncHandler!.GetInvocationList().Length;

            sut.OnFailedAsync += OnFailedAsync;
            castedSut.OnFailedAsync += OnFailedAsync;

            Assert.NotNull(eventStore.OnFailedAsyncHandler);
            Assert.Equal(initialDelegateCount + 2, eventStore.OnFailedAsyncHandler!.GetInvocationList().Length);

            eventStore.OnFailedAsyncHandler!(this, args);
            Assert.True(eventCalled);

            sut.OnFailedAsync -= OnFailedAsync;
            castedSut.OnFailedAsync -= OnFailedAsync;
            eventCalled = false;

            Assert.Equal(initialDelegateCount, eventStore.OnFailedAsyncHandler!.GetInvocationList().Length);

            eventStore.OnFailedAsyncHandler!(this, args);

            Assert.False(eventCalled);
        }

        [MoqAutoData]
        [Theory]
        public void EnsureOnComponentUpdateEventIsCalled([Frozen] InMemoryEventStore<TSuccess, TFailure> eventStore,
            InMemoryEventReceiver<TSuccess, TFailure> sut, ComponentEventArgs<ComponentStatus> args)
        {
            IEventReceiver castedSut = sut;
            var eventCalled = false;

            Task OnComponentUpdate(object? sender, ComponentEventArgs<ComponentStatus> e)
            {
                Assert.Equal(this, sender);
                Assert.Equal(args, e);

                eventCalled = true;
                return Task.CompletedTask;
            }

            var initialDelegateCount = eventStore.OnComponentUpdateAsyncHandler!.GetInvocationList().Length;

            sut.OnComponentUpdateAsync += OnComponentUpdate;

            Assert.NotNull(eventStore.OnComponentUpdateAsyncHandler);
            Assert.Equal(initialDelegateCount + 1, eventStore.OnComponentUpdateAsyncHandler!.GetInvocationList().Length);

            eventStore.OnComponentUpdateAsyncHandler!(this, args);
            Assert.True(eventCalled);

            sut.OnComponentUpdateAsync -= OnComponentUpdate;
            eventCalled = false;

            Assert.Equal(initialDelegateCount, eventStore.OnComponentUpdateAsyncHandler!.GetInvocationList().Length);

            eventStore.OnComponentUpdateAsyncHandler!(this, args);
            Assert.False(eventCalled);
        }
    }
}
