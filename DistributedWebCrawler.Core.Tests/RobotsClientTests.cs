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

        [HttpClientAutoData(content: "nonEmpty")]
        [Theory]
        public async Task TryGetRobotsShouldReturnTrueWhenContentIsReturned(RobotsClient sut)
        {
            await TryGetRobotsTest(sut, expectedReturnValue: true);
        }

        [HttpClientAutoData(content: "")]
        [Theory]
        public async Task TryGetRobotsShouldReturnFalseWhenContentIsEmpty(RobotsClient sut)
        {
            await TryGetRobotsTest(sut, expectedReturnValue: false);
        }

        [HttpClientAutoData(statusCode: HttpStatusCode.NotFound)]
        [Theory]
        public async Task TryGetRobotsShouldReturnFalseWhenHttpStatusCodeIsNotOk(RobotsClient sut)
        {
            await TryGetRobotsTest(sut, expectedReturnValue: false);
        }

        [ExceptionThrowingHttpClientAutoData]
        [Theory]
        public async Task TryGetRobotsShouldReturnFalseWhenHttpExceptionThrown(RobotsClient sut)
        {
            await TryGetRobotsTest(sut, expectedReturnValue: false);
        }

        [CancelledHttpClientAutoData]
        [Theory]
        public async Task TryGetRobotsShouldReturnFalseWhenHttpConnectionTimesOut(RobotsClient sut)
        {
            await TryGetRobotsTest(sut, expectedReturnValue: false);
        }

        private async Task TryGetRobotsTest(RobotsClient sut, bool expectedReturnValue)
        {
            var callbackCalled = new CallbackSentinel();

            var result = await sut.TryGetRobotsAsync(MockUri, GetRobotsCallback(callbackCalled), _cts.Token);

            Assert.Equal(expectedReturnValue, result);
            Assert.Equal(expectedReturnValue, callbackCalled.Value);
        }

        private static Func<string, Task> GetRobotsCallback(CallbackSentinel sentinel)
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
