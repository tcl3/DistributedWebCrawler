using AutoFixture.Xunit2;
using DistributedWebCrawler.Core.Enums;
using DistributedWebCrawler.Core.Interfaces;
using DistributedWebCrawler.Core.Models;
using DistributedWebCrawler.Core.Tests.Attributes;
using DistributedWebCrawler.Core.Tests.Fakes;
using Moq;
using System;
using System.Linq.Expressions;
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
            Assert.Equal(CrawlerComponentStatus.NotStarted, sut.Status);

            await sut.StartAsync(CrawlerRunningState.Paused, cts.Token);
            
            Assert.Equal(CrawlerComponentStatus.Paused, sut.Status);
            
            await WaitForCancellationAsync(sut);

            consumerMock.Verify(x => x.DequeueAsync(), Times.Never());
        }

        [Theory]
        [TaskQueueAutoData(throwsException: true, numberOfItemsToDequeue: 2)]
        public async Task ExceptionsThrownByRequestProcessorShouldBeCaught(
            [Frozen] CancellationTokenSource cts,
            [Frozen] Mock<IConsumer<TestRequest>> consumerMock,
            TestComponent sut)
        {
            await sut.StartAsync(CrawlerRunningState.Running, cts.Token);
            
            Assert.Equal(CrawlerComponentStatus.Running, sut.Status);
            
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
            
            Assert.Equal(CrawlerComponentStatus.Paused, sut.Status);
            
            cts.Cancel();

            await sut.ResumeAsync();

            Assert.Equal(CrawlerComponentStatus.Running, sut.Status);

            await WaitForCancellationAsync(sut);

            consumerMock.Verify(x => x.DequeueAsync(), Times.Never());
        }

        [Theory]
        [TaskQueueAutoData(resultStatus: QueuedItemStatus.Success, numberOfItemsToDequeue: 1)]
        public async Task WhenRequestProcessorReturnsSuccessNotifierShouldBeCalled(
            [Frozen] Mock<IEventDispatcher<TestSuccess, ErrorCode<TestFailure>>> eventDispatcherMock,
            [Frozen] TestRequest request,
            [Frozen] CancellationTokenSource cts,
            [Frozen] NodeStatus nodeStatus,
            TestComponent sut)
        {
            await sut.StartAsync(CrawlerRunningState.Running, cts.Token);

            Assert.Equal(CrawlerComponentStatus.Running, sut.Status);

            await WaitForCancellationAsync(sut);

            eventDispatcherMock.Verify(x => x.NotifyCompletedAsync(request, It.IsAny<ComponentInfo>(), It.IsAny<TestSuccess>()), Times.Once());
            eventDispatcherMock.Verify(x => x.NotifyComponentStatusUpdateAsync(It.IsAny<ComponentInfo>(), It.Is(IsValidComponentStatus(nodeStatus))), Times.Once());
        }

        //[Theory]
        //[TaskQueueAutoData(resultStatus: QueuedItemStatus.Success, numberOfItemsToDequeue: 2, maxConcurrentItems: 2)]
        //public async Task WhenRequestProcessorReturnsSuccessNotifierShouldBeCalled2(
        //    [Frozen] Mock<IEventDispatcher<TestSuccess, ErrorCode<TestFailure>>> eventDispatcherMock,
        //    [Frozen] TestRequest request,
        //    [Frozen] CancellationTokenSource cts,
        //    [Frozen] NodeStatus nodeStatus,
        //    TestComponent sut)
        //{
        //    var invocationCount = 0;
        //    var expectedTasksInUse = true;
        //    eventDispatcherMock.Setup(x => x.NotifyComponentStatusUpdateAsync(sut.ComponentInfo, It.IsAny<ComponentStatus>()))
        //        .Callback<ComponentInfo, ComponentStatus>((info, status) =>
        //        {
        //            invocationCount++;
        //            expectedTasksInUse = expectedTasksInUse && status.TasksInUse == invocationCount;
        //        });

        //    await sut.StartAsync(CrawlerRunningState.Running, cts.Token);
        //    Assert.Equal(CrawlerComponentStatus.Running, sut.Status);

        //    await WaitForCancellationAsync(sut);

        //    Assert.True(expectedTasksInUse);

        //    eventDispatcherMock.Verify(x => x.NotifyCompletedAsync(request, sut.ComponentInfo, It.IsAny<TestSuccess>()), Times.Exactly(2));
        //    eventDispatcherMock.Verify(x => x.NotifyComponentStatusUpdateAsync(It.IsAny<ComponentInfo>(), It.Is(IsValidComponentStatus(nodeStatus))), Times.Exactly(2));
        //}

        [Theory]
        [TaskQueueAutoData(resultStatus: QueuedItemStatus.Failed, numberOfItemsToDequeue: 1)]
        public async Task WhenRequestProcessorReturnsFailureNotifierShouldBeCalled(
            [Frozen] Mock<IEventDispatcher<TestSuccess, ErrorCode<TestFailure>>> eventDispatcherMock,
            [Frozen] TestRequest request,
            [Frozen] CancellationTokenSource cts,
            [Frozen] NodeStatus nodeStatus,
            TestComponent sut)
        {
            await sut.StartAsync(CrawlerRunningState.Running, cts.Token);
            
            Assert.Equal(CrawlerComponentStatus.Running, sut.Status);

            await WaitForCancellationAsync(sut);

            eventDispatcherMock.Verify(x => x.NotifyFailedAsync(request, sut.ComponentInfo, It.IsAny<ErrorCode<TestFailure>>()), Times.Once());
            eventDispatcherMock.Verify(x => x.NotifyComponentStatusUpdateAsync(sut.ComponentInfo, It.Is(IsValidComponentStatus(nodeStatus))), Times.Once());
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

            Assert.Equal(CrawlerComponentStatus.Running, sut.Status);

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
        private static Expression<Func<ComponentStatus, bool>> IsValidComponentStatus(NodeStatus nodeStatus)
        {
            return c => c.QueueCount == 0 && c.NodeStatus == nodeStatus;
        }
    }
}
