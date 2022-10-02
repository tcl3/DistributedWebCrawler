using AutoFixture.Xunit2;
using DistributedWebCrawler.Core.Interfaces;
using DistributedWebCrawler.Core.Models;
using DistributedWebCrawler.Core.Queue;
using DistributedWebCrawler.Core.Tests.Attributes;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace DistributedWebCrawler.Core.Tests
{
    public class SchedulerIngestQueueTests
    {
        [Theory]
        [SchedulerIngestQueueAutoData(expectedIngestQueueInvocations: 1)]
        public async Task Test(
            [Frozen] SchedulerRequest schedulerRequest,
            [Frozen] IEnumerable<string> pathsToVisit,
            [Frozen] SemaphoreSlim ingestQueueInvocationSemaphore,
            CancellationToken cancellationToken,
            SchedulerIngestQueue sut)
        {
            var urisToVisit = MakeRelativeUris(schedulerRequest.Uri, pathsToVisit);
            await sut.AddFromSchedulerAsync(schedulerRequest, urisToVisit, cancellationToken);

            await ingestQueueInvocationSemaphore.WaitAsync(cancellationToken);
        }

        [Theory]
        [SchedulerIngestQueueAutoData(expectedIngestQueueInvocations: 1)]
        public async Task DisposeTest(
            [Frozen] SchedulerRequest schedulerRequest,
            [Frozen] IEnumerable<string> pathsToVisit,
            CancellationToken cancellationToken,
            SchedulerIngestQueue sut)
        {
            sut.Dispose();
            var urisToVisit = MakeRelativeUris(schedulerRequest.Uri, pathsToVisit);
            await Assert.ThrowsAsync<ObjectDisposedException>(() => sut.AddFromSchedulerAsync(schedulerRequest, urisToVisit, cancellationToken));
        }

        [Theory]
        [SchedulerIngestQueueAutoData(expectedIngestQueueInvocations: 1, ingestCallbackSuccessful: false, timeoutMillis: 10000)]
        public async Task Test11(

            [Frozen] SchedulerRequest schedulerRequest,
            [Frozen] IEnumerable<string> pathsToVisit,
            [Frozen] SemaphoreSlim ingestQueueInvocationSemaphore,
            [Frozen] CancellationToken cancellationToken,
            SchedulerIngestQueue sut)
        {
            await sut.AddFromSchedulerAsync(schedulerRequest, MakeRelativeUris(schedulerRequest.Uri, pathsToVisit), cancellationToken);

            await ingestQueueInvocationSemaphore.WaitAsync(cancellationToken);

        }

        [Theory]
        [SchedulerIngestQueueAutoData(expectedIngestQueueInvocations: 1)]
        public async Task Test2(
            [Frozen] SchedulerRequest schedulerRequest,
            [Frozen] IEnumerable<string> pathsToVisit,
            [Frozen] SemaphoreSlim ingestQueueInvocationSemaphore,
            [Frozen] CancellationToken cancellationToken,
            SchedulerIngestQueue sut)
        {
            var urisToVisit = MakeRelativeUris(schedulerRequest.Uri, pathsToVisit);

            var task1 = sut.AddFromSchedulerAsync(schedulerRequest, urisToVisit, cancellationToken);
            var task2 = sut.AddFromSchedulerAsync(schedulerRequest, urisToVisit, cancellationToken);

            await Task.WhenAll(task1, task2);
            await ingestQueueInvocationSemaphore.WaitAsync(cancellationToken);
        }

        [Theory]
        [SchedulerIngestQueueAutoData(expectedIngestQueueInvocations: 1)]
        public async Task Test3(
            SchedulerRequest schedulerRequest1,
            SchedulerRequest schedulerRequest2,
            IEnumerable<string> pathsToVisit1,
            IEnumerable<string> pathsToVisit2,
            [Frozen] SemaphoreSlim ingestQueueInvocationSemaphore,
            [Frozen] CancellationToken cancellationToken,
            SchedulerIngestQueue sut)
        {
            var urisToVisit1 = MakeRelativeUris(schedulerRequest1.Uri, pathsToVisit1);
            var task1 = sut.AddFromSchedulerAsync(schedulerRequest1, urisToVisit1, cancellationToken);

            var urisToVisit2 = MakeRelativeUris(schedulerRequest2.Uri, pathsToVisit2);
            var task2 = sut.AddFromSchedulerAsync(schedulerRequest2, urisToVisit2, cancellationToken);

            await Task.WhenAll(task1, task2);
            await ingestQueueInvocationSemaphore.WaitAsync(cancellationToken);
        }

        //[Theory]
        //[SchedulerIngestQueueAutoData(expectedIngestQueueInvocations: 1)]
        //public async Task Test8(
        //    [Frozen] Mock<IProducer<IngestRequest>> ingestProducerMock,
        //    [Frozen] Mock<IEventReceiver<IngestSuccess, IngestFailure>> ingestEventReceiverMock,
        //    [Frozen] SchedulerRequest schedulerRequest,
        //    [Frozen] IEnumerable<Uri> urisToVisit,
        //    [Frozen] IngestSuccess ingestSuccess,
        //    [Frozen] ComponentInfo nodeInfo,
        //    SchedulerIngestQueue sut)
        //{
        //    Guid? ingestRequestId = null;
        //    var semaphore = new SemaphoreSlim(0);
        //    ingestProducerMock.Setup(x => x.Enqueue(It.IsAny<IngestRequest>()))
        //        .Callback<IngestRequest>(request =>
        //        {
        //            ingestRequestId = request.Id;
        //            semaphore.Release();
        //        });

        //    var cts = new CancellationTokenSource(TimeSpan.FromSeconds(1));

        //    await sut.AddFromSchedulerAsync(schedulerRequest, urisToVisit, cts.Token);

        //    await semaphore.WaitAsync(cts.Token);

        //    Assert.False(cts.IsCancellationRequested);
        //    Assert.NotNull(ingestRequestId);

        //    var eventArgs = new ItemCompletedEventArgs<IngestSuccess>(ingestRequestId!.Value, nodeInfo, ingestSuccess);

        //    ingestEventReceiverMock.Raise(x => x.OnCompletedAsync += null, this, eventArgs);

        //    await Task.Delay(TimeSpan.FromMilliseconds(10));
        //}

        //[Theory]
        //[MoqAutoData]
        //public async Task Test9(
        //    [Frozen] Mock<IProducer<IngestRequest>> ingestProducerMock,
        //    [Frozen] Mock<IEventReceiver<IngestSuccess, IngestFailure>> ingestEventReceiverMock,
        //    [Frozen] SchedulerRequest schedulerRequest,
        //    [Frozen] IEnumerable<Uri> urisToVisit,
        //    [Frozen] IngestFailure ingestFailure,
        //    [Frozen] ComponentInfo nodeInfo,
        //    SchedulerIngestQueue sut)
        //{
        //    Guid? ingestRequestId = null;
        //    var semaphore = new SemaphoreSlim(0);
        //    ingestProducerMock.Setup(x => x.Enqueue(It.IsAny<IngestRequest>()))
        //        .Callback<IngestRequest>(request =>
        //        {
        //            semaphore.Release();
        //            ingestRequestId = request.Id;
        //        });

        //    var cts = new CancellationTokenSource(TimeSpan.FromSeconds(1));
        //    await sut.AddFromSchedulerAsync(schedulerRequest, urisToVisit, cts.Token);

        //    await semaphore.WaitAsync(cts.Token);

        //    Assert.False(cts.IsCancellationRequested);
        //    Assert.NotNull(ingestRequestId);

        //    var eventArgs = new ItemFailedEventArgs<IngestFailure>(ingestRequestId!.Value, nodeInfo, ingestFailure);

        //    ingestEventReceiverMock.Raise(x => x.OnFailedAsync += null, this, eventArgs);
        //}

        private static IEnumerable<Uri> MakeRelativeUris(Uri baseUri, IEnumerable<string> pathsToVisit)
        {
            return pathsToVisit.Select(path => new Uri(baseUri, path));
        }
    }
}
