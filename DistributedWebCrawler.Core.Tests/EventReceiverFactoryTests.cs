using AutoFixture;
using AutoFixture.Xunit2;
using DistributedWebCrawler.Core.Interfaces;
using DistributedWebCrawler.Core.Models;
using DistributedWebCrawler.Core.Tests.Attributes;
using DistributedWebCrawler.Core.Tests.Fakes;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
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

            result.Should().BeEquivalentTo(eventReceivers);
        }

        [MoqAutoData]
        [Theory]
        public void GetAllShouldReturnAllEventReceiversWithNoDuplicates([Frozen] IEnumerable<IEventReceiver> eventReceivers, IFixture fixture)
        {
            fixture.Inject<Func<Type, object>>(type =>
            {
                Assert.True(type.IsAssignableFrom(typeof(IEnumerable<IEventReceiver>)));

                return Enumerable.Repeat(eventReceivers, 2).SelectMany(x => x);
            });

            var sut = fixture.Create<EventReceiverFactory>();

            var result = sut.GetAll();

            result.Should().BeEquivalentTo(eventReceivers);
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
