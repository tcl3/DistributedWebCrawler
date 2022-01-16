using AutoFixture;
using AutoFixture.Xunit2;
using DistributedWebCrawler.Core.Interfaces;
using DistributedWebCrawler.Core.Model;
using DistributedWebCrawler.Core.Tests.Attributes;
using DistributedWebCrawler.Core.Tests.Fakes;
using System;
using System.Collections.Generic;
using Xunit;

namespace DistributedWebCrawler.Core.Tests
{
    public class EventReceiverFactoryTests
    {
        [MoqAutoData]
        [Theory]
        public void GetAllShouldReturnAllEventReceivers([Frozen] IEnumerable<IEventReceiver> eventReceivers, IFixture fixture)
        {
            fixture.Inject<Func<Type, object>>(type => 
            {
                Assert.True(type.IsAssignableFrom(typeof(IEnumerable<IEventReceiver>)));
                
                return eventReceivers;
            });
            
            var sut = fixture.Create<EventReceiverFactory>();

            var result = sut.GetAll();

            Assert.Equal(eventReceivers, result);
        }

        [MoqAutoData]
        [Theory]
        public void GetShouldReturnEventReceiverOfCorrectType([Frozen] IEventReceiver<TestSuccess, ErrorCode<TestFailure>> eventReceiver, IFixture fixture)
        {
            fixture.Inject<Func<Type, object>>(type =>
            {
                Assert.True(type.IsAssignableFrom(typeof(IEventReceiver<TestSuccess, ErrorCode<TestFailure>>)));
                return eventReceiver;
            });

            var sut = fixture.Create<EventReceiverFactory>();

            var result = sut.Get<TestSuccess, ErrorCode<TestFailure>>();

            Assert.Equal(eventReceiver, result);
        }
    }
}
