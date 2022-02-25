using AutoFixture.Xunit2;
using DistributedWebCrawler.Core.Tests.Attributes;
using DistributedWebCrawler.Core.Tests.Fakes;
using Moq;
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
        private const string MockUriString = "http://mock.test/";
        private const string MockUriStringWithRobotsPath = MockUriString + "robots.txt";

        [HttpClientAutoData(allowedUris: new[] { MockUriStringWithRobotsPath }, content: "nonEmpty")]
        [Theory]
        public async Task TryGetRobotsShouldReturnTrueWhenContentIsReturned(RobotsClient sut)
        {
            await TryGetRobotsTest(sut, expectedReturnValue: true);
        }

        [HttpClientAutoData(allowedUris: new[] { MockUriStringWithRobotsPath }, content: "")]
        [Theory]
        public async Task TryGetRobotsShouldReturnFalseWhenContentIsEmpty(RobotsClient sut)
        {
            await TryGetRobotsTest(sut, expectedReturnValue: false);
        }

        [HttpClientAutoData(allowedUris: new[] { MockUriStringWithRobotsPath }, statusCode: HttpStatusCode.NotFound)]
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

        private static async Task TryGetRobotsTest(RobotsClient sut, bool expectedReturnValue)
        {
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(1));
            var callbackCalled = new CallbackSentinel();

            var cancellationToken = cts.Token;

            var result = await sut.TryGetRobotsAsync(new Uri(MockUriString), GetRobotsCallback(callbackCalled), cancellationToken);
            
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
