using AutoFixture.Xunit2;
using DistributedWebCrawler.Core.Components;
using DistributedWebCrawler.Core.Interfaces;
using DistributedWebCrawler.Core.Tests.Attributes;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace DistributedWebCrawler.Core.Tests
{
    public class CompositeEventReceiverTests
    {
        [MoqAutoData]
        [Theory]
        public void OnCompletedShouldDelegateToUnderlyingEventReceivers(
            [Frozen] IEnumerable<IEventReceiver> eventReceivers,
            CompositeEventReceiver sut)
        {
            var callCount = 0;
            ItemCompletedEventHandler onCompletedEvent = (sender, e) =>
            {
                callCount++;
                return Task.CompletedTask;
            };

            sut.OnCompletedAsync += onCompletedEvent;
            RaiseEvent(eventReceivers, x => x.OnCompletedAsync += null);
            Assert.Equal(eventReceivers.Count(), callCount);

            callCount = 0;

            sut.OnCompletedAsync -= onCompletedEvent;
            RaiseEvent(eventReceivers, x => x.OnCompletedAsync += null);
            Assert.Equal(0, callCount);
        }

        [MoqAutoData]
        [Theory]
        public void OnFailedShouldDelegateToUnderlyingEventReceivers(
            [Frozen] IEnumerable<IEventReceiver> eventReceivers,
            CompositeEventReceiver sut)
        {
            var callCount = 0;
            ItemFailedEventHandler onFailedEvent = (sender, e) =>
            {
                callCount++;
                return Task.CompletedTask;
            };

            sut.OnFailedAsync += onFailedEvent;
            RaiseEvent(eventReceivers, x => x.OnFailedAsync += null);
            Assert.Equal(eventReceivers.Count(), callCount);

            callCount = 0;

            sut.OnFailedAsync -= onFailedEvent;
            RaiseEvent(eventReceivers, x => x.OnFailedAsync += null);
            Assert.Equal(0, callCount);
        }

        [MoqAutoData]
        [Theory]
        public void OnComponentUpdateShouldDelegateToUnderlyingEventReceivers(
            [Frozen] IEnumerable<IEventReceiver> eventReceivers,
            CompositeEventReceiver sut)
        {
            var callCount = 0;
            ComponentEventHandler<ComponentStatus> onComponentUpdate = (sender, e) =>
            {
                callCount++;
                return Task.CompletedTask;
            };

            sut.OnComponentUpdateAsync += onComponentUpdate;
            RaiseEvent(eventReceivers, x => x.OnComponentUpdateAsync += null);
            Assert.Equal(eventReceivers.Count(), callCount);

            callCount = 0;

            sut.OnComponentUpdateAsync -= onComponentUpdate;
            RaiseEvent(eventReceivers, x => x.OnComponentUpdateAsync += null);
            Assert.Equal(0, callCount);
        }

        private void RaiseEvent(IEnumerable<IEventReceiver> eventReceivers, Action<IEventReceiver> eventExpression)
        {
            foreach (var eventReceiver in eventReceivers)
            {
                var eventReceiverMock = Mock.Get(eventReceiver);
                eventReceiverMock.Raise(eventExpression, this, null);
            }
        }
    }
}
