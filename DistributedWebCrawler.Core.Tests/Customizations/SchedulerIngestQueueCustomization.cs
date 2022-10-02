using AutoFixture;
using DistributedWebCrawler.Core.Interfaces;
using DistributedWebCrawler.Core.Models;
using DistributedWebCrawler.Core.Queue;
using Moq;
using System;
using System.Threading;
using DistributedWebCrawler.Core;
using System.Linq;

namespace DistributedWebCrawler.Core.Tests.Customizations
{
    internal class SchedulerIngestQueueCustomization : ICustomization
    {
        private readonly int _expectedIngestQueueInvocations;
        private readonly TimeSpan _timeout;
        private readonly bool _ingestCallbackSuccessful;

        public SchedulerIngestQueueCustomization(int expectedIngestQueueInvocations, TimeSpan timeout, bool ingestCallbackSuccessful)
        {
            _expectedIngestQueueInvocations = expectedIngestQueueInvocations;
            _timeout = timeout;
            _ingestCallbackSuccessful = ingestCallbackSuccessful;
        }

        public void Customize(IFixture fixture)
        {
            var ingestEventReceiverMock = fixture.Freeze<Mock<IEventReceiver<IngestSuccess, IngestFailure>>>();

            fixture.Customize<SchedulerIngestQueue.SchedulerQueueEntry>(c => c
                .With(x => x.Domain, () => fixture.Create<SchedulerRequest>().Uri.Host)
                .With(x => x.Uri, () => 
                {
                    var schedulerRequest = fixture.Create<SchedulerRequest>();
                    return new Uri(schedulerRequest.Uri, schedulerRequest.Paths.First());
                }));
            
            var cts = new CancellationTokenSource(_timeout);
            fixture.Inject(cts.Token);

            var invocationSemaphore = new SemaphoreSlim(0);
            fixture.Inject(invocationSemaphore);

            var currentInvocationCount = 0;
            var ingestProducerMock = new Mock<IProducer<IngestRequest>>();
            ingestProducerMock
                .Setup(x => x.Enqueue(It.IsAny<IngestRequest>()))
                .Callback((IngestRequest request) =>
                {
                    if (currentInvocationCount > _expectedIngestQueueInvocations)
                    {
                        return;
                    }

                    if (_ingestCallbackSuccessful)
                    {
                        var eventArgs = new ItemCompletedEventArgs<IngestSuccess>(request.Id, fixture.Create<ComponentInfo>(), fixture.Create<IngestSuccess>());
                        ingestEventReceiverMock.Raise(x => x.OnCompletedAsync += null, this, eventArgs);
                    }
                    else
                    {
                        var eventArgs = new ItemFailedEventArgs<IngestFailure>(request.Id, fixture.Create<ComponentInfo>(), fixture.Create<IngestFailure>());
                        ingestEventReceiverMock.Raise(x => x.OnFailedAsync += null, this, eventArgs);
                    }                    

                    var count = Interlocked.Increment(ref currentInvocationCount);
                    if (count >= _expectedIngestQueueInvocations)
                    {
                        invocationSemaphore.Release();
                    }
                });

            fixture.Inject(ingestProducerMock);
        }
    }
}
