using AutoFixture.Xunit2;
using DistributedWebCrawler.Core.Tests.Attributes;
using DistributedWebCrawler.Core.Tests.Fakes;
using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace DistributedWebCrawler.Core.Tests
{
    public class RobotsClientTests
    {
        private static readonly Uri MockUri = new Uri("http://mock.test/");
        private readonly CancellationTokenSource _cts = new(TimeSpan.FromSeconds(1));

        [HttpClientAutoData]
        [Theory]
        public async Task TryGetRobotsShouldReturnTrueWhenContentIsReturned([Frozen] HttpResponseMessage response, RobotsClient sut)
        {
            var callbackCalled = new CallbackSentinel();

            response.StatusCode = HttpStatusCode.OK;
            response.Content = new StringContent("nonEmpty");

            await sut.TryGetRobotsAsync(MockUri, GetRobotsCallback(callbackCalled), _cts.Token);

            Assert.True(callbackCalled.Value);
        }

        [HttpClientAutoData]
        [Theory]
        public async Task TryGetRobotsShouldReturnFalseWhenContentIsEmpty([Frozen] HttpResponseMessage response, RobotsClient sut)
        {
            var callbackCalled = new CallbackSentinel();

            response.StatusCode = HttpStatusCode.OK;
            response.Content = new StringContent("");

            await sut.TryGetRobotsAsync(MockUri, GetRobotsCallback(callbackCalled), _cts.Token);

            Assert.False(callbackCalled.Value);
        }

        [HttpClientAutoData]
        [Theory]
        public async Task TryGetRobotsShouldReturnFalseWhenHttpStatusCodeIsNotOk([Frozen] HttpResponseMessage response, RobotsClient sut)
        {
            var callbackCalled = new CallbackSentinel();

            response.StatusCode = HttpStatusCode.NotFound;

            await sut.TryGetRobotsAsync(MockUri, GetRobotsCallback(callbackCalled), _cts.Token);

            Assert.False(callbackCalled.Value);
        }

        [HttpClientAutoData]
        [Theory]
        public async Task TryGetRobotsShouldReturnFalseWhenHttpExceptionThrown([Frozen] FakeHttpMessageHandler testMessageHandler, RobotsClient sut)
        {
            var callbackCalled = new CallbackSentinel();

            testMessageHandler.SetException(new HttpRequestException("Test exception"));

            await sut.TryGetRobotsAsync(MockUri, GetRobotsCallback(callbackCalled), _cts.Token);

            Assert.False(callbackCalled.Value);
        }

        private Func<string, Task> GetRobotsCallback(CallbackSentinel sentinel)
        {
            return robotsContent =>
            {
                sentinel.Called();
                return Task.CompletedTask;
            };
        }

        private class CallbackSentinel
        {
            public bool Value { get; private set; } = false;

            public void Called()
            {
                Value = true;
            }
        }
    }
}
