using AutoFixture.Xunit2;
using DistributedWebCrawler.Core.Enums;
using DistributedWebCrawler.Core.Interfaces;
using DistributedWebCrawler.Core.Model;
using DistributedWebCrawler.Core.Models;
using DistributedWebCrawler.Core.Tests.Attributes;
using DistributedWebCrawler.Core.Tests.Fakes;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace DistributedWebCrawler.Core.Tests
{
    public class TaskQueueComponentTests
    {
        [Theory]
        [TaskQueueAutoData]
        public async Task CallingPauseBeforeStartShouldThrowException(TestComponent sut)
        {
            await Assert.ThrowsAsync<InvalidOperationException>(() => sut.PauseAsync());
        }

        [Theory]
        [TaskQueueAutoData]
        public async Task CallingResumeBeforeStartShouldThrowException(TestComponent sut)
        {
            await Assert.ThrowsAsync<InvalidOperationException>(() => sut.ResumeAsync());
        }

        [Theory]
        [TaskQueueAutoData(cancelAfterMilliseconds: 1)]
        public async Task CallingStartWithPausedStateShouldNotProcessAnyItems(
            [Frozen] Mock<IConsumer<TestRequest>> consumerMock,
            [Frozen] CancellationTokenSource cts,
            TestComponent sut)
        {
            await sut.StartAsync(CrawlerRunningState.Paused, cts.Token);

            await WaitForCancellationAsync(sut);

            consumerMock.Verify(x => x.DequeueAsync(), Times.Never());
        }

        [Theory]
        [TaskQueueAutoData(throwsException: true, numberOfItemsToDequeue: 2)]
        public async Task RequestProcessorThrowsException(
            [Frozen] CancellationTokenSource cts,
            [Frozen] Mock<IConsumer<TestRequest>> consumerMock,
            TestComponent sut)
        {
            await sut.StartAsync(CrawlerRunningState.Running, cts.Token);

            await WaitForCancellationAsync(sut);

            consumerMock.Verify(x => x.DequeueAsync(), Times.Exactly(2));
        }

        [Theory]
        [TaskQueueAutoData]
        public async Task CancellingWhilePausedShouldExitThreadWithoutDequeueingItem(
            [Frozen] Mock<IConsumer<TestRequest>> consumerMock,
            [Frozen] CancellationTokenSource cts,
            TestComponent sut)
        {   
            await sut.StartAsync(CrawlerRunningState.Paused, cts.Token);

            // Delay needed to ensure pause semaphore is entered
            await Task.Delay(1);

            cts.Cancel();

            await sut.ResumeAsync();

            await WaitForCancellationAsync(sut);

            consumerMock.Verify(x => x.DequeueAsync(), Times.Never());
        }        

        [Theory]
        [TaskQueueAutoData(resultStatus: QueuedItemStatus.Success, numberOfItemsToDequeue: 1)]
        public async Task WhenRequestProcessorReturnsSuccessNotifierShouldBeCalled(
            [Frozen] Mock<IEventDispatcher<TestSuccess, ErrorCode<TestFailure>>> eventDispatcherMock,
            [Frozen] TestRequest request,
            [Frozen] CancellationTokenSource cts,
            TestComponent sut)
        {
            await sut.StartAsync(CrawlerRunningState.Running, cts.Token);

            await WaitForCancellationAsync(sut);

            eventDispatcherMock.Verify(x => x.NotifyCompletedAsync(request, It.IsAny<NodeInfo>(), It.IsAny<TestSuccess>()), Times.Once());
        }

        [Theory]
        [TaskQueueAutoData(resultStatus: QueuedItemStatus.Failed, numberOfItemsToDequeue: 1)]
        public async Task WhenRequestProcessorReturnsFailureNotifierShouldBeCalled(
            [Frozen] Mock<IEventDispatcher<TestSuccess, ErrorCode<TestFailure>>> eventDispatcherMock,
            [Frozen] TestRequest request,
            [Frozen] CancellationTokenSource cts,
            TestComponent sut)
        {
            await sut.StartAsync(CrawlerRunningState.Running, cts.Token);

            await WaitForCancellationAsync(sut);

            eventDispatcherMock.Verify(x => x.NotifyFailedAsync(request, It.IsAny<NodeInfo>(), It.IsAny<ErrorCode<TestFailure>>()), Times.Once());
        }

        [Theory]
        [TaskQueueAutoData(resultStatus: QueuedItemStatus.Waiting, numberOfItemsToDequeue: 1)]
        public async Task WhenRequestProcessorReturnsWaitingKeyStoreShouldBeCalled(
            [Frozen] Mock<IKeyValueStore> keyValueStoreMock,
            [Frozen] TestRequest request,
            [Frozen] CancellationTokenSource cts,
            TestComponent sut)
        {
            await sut.StartAsync(CrawlerRunningState.Running, cts.Token);

            await WaitForCancellationAsync(sut);

            keyValueStoreMock.Verify(x => x.PutAsync(It.IsAny<string>(), request, It.IsAny<TimeSpan?>()));
        }

        [Theory]
        [TaskQueueAutoData]
        public async Task StartAfterAlreadyStartedShouldThrowException(TestComponent sut)
        {
            await sut.StartAsync();
            await Assert.ThrowsAsync<InvalidOperationException>(() => sut.StartAsync());
        }

        [Theory]
        [TaskQueueAutoData(numberOfItemsToDequeue: 1)]
        public async Task ResumeAfterStartPausedShouldDequeueItems(
            [Frozen] Mock<IConsumer<TestRequest>> consumerMock,
            [Frozen] CancellationTokenSource cts,
            TestComponent sut)
        {            
            await sut.StartAsync(CrawlerRunningState.Paused, cts.Token);

            // Delay needed to ensure pause semaphore is entered
            await Task.Delay(TimeSpan.FromMilliseconds(1));

            consumerMock.Verify(x => x.DequeueAsync(), Times.Never());

            await sut.ResumeAsync();

            await WaitForCancellationAsync(sut);

            consumerMock.Verify(x => x.DequeueAsync(), Times.Once());
        }

        private static async Task WaitForCancellationAsync(TestComponent component)
        {
            await Assert.ThrowsAsync<TaskCanceledException>(() => component.WaitUntilCompletedAsync());
        }
    }
}
