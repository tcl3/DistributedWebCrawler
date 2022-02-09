using AngleSharp.Io.Processors;
using AutoFixture;
using DistributedWebCrawler.Core.Extensions;
using DistributedWebCrawler.Core.Interfaces;
using DistributedWebCrawler.Core.Models;
using DistributedWebCrawler.Core.Tests.Fakes;
using Moq;
using System;
using System.Threading;

namespace DistributedWebCrawler.Core.Tests.Customizations
{
    internal class TaskQueueCustomization : ICustomization
    {
        private readonly QueuedItemStatus _resultStatus;
        private readonly bool _throwsException;
        private readonly int _numberOfItemsToDequeue;
        private readonly int _cancelAfterMilliseconds;

        public TaskQueueCustomization(
            QueuedItemStatus resultStatus = QueuedItemStatus.Success, 
            bool throwsException = false,
            int numberOfItemsToDequeue = 0,
            int cancelAfterMilliseconds = 1000)
        {
            _resultStatus = resultStatus;
            _throwsException = throwsException;
            _numberOfItemsToDequeue = numberOfItemsToDequeue;
            _cancelAfterMilliseconds = cancelAfterMilliseconds;
        }

        public void Customize(IFixture fixture)
        {
            fixture.Register(() => new CancellationTokenSource(TimeSpan.FromMilliseconds(_cancelAfterMilliseconds)));

            fixture.Customize<TestSettings>(c => c.With(x => x.MaxConcurrentItems, 1));
            
            var componentDescriptor = new ComponentDescriptor(null!, null!, null!, "Test");
            fixture.Inject(componentDescriptor);

            var componentNameProviderMock = fixture.Freeze<Mock<IComponentNameProvider>>();
            componentNameProviderMock
                .Setup(x => x.TryGetFromComponentType(It.IsAny<Type>(), out componentDescriptor))
                .Returns(() => true);

            fixture.Inject(componentNameProviderMock.Object);

            fixture.Customize<QueuedItemResult>(c => c.FromFactory((TestRequest request) =>
            {
                switch (_resultStatus)
                {
                    case QueuedItemStatus.Success:
                        return request.Success(fixture.Create<TestSuccess>());
                    case QueuedItemStatus.Failed:
                        return request.Failed(fixture.Create<ErrorCode<TestFailure>>());
                    case QueuedItemStatus.Waiting:
                        return request.Waiting();
                }
                
                throw new InvalidOperationException($"QueuedItemStatus {_resultStatus} not supported");
            })
            .OmitAutoProperties());

            if (_throwsException)
            {
                var requestProcessorMock = new Mock<IRequestProcessor<TestRequest>>();
                requestProcessorMock
                    .Setup(x => x.ProcessItemAsync(It.IsAny<TestRequest>(), It.IsAny<CancellationToken>()))
                    .Throws<Exception>();

                fixture.Inject(requestProcessorMock.Object);
            }

            if (_numberOfItemsToDequeue > 0)
            {
                var cts = fixture.Freeze<CancellationTokenSource>();
                var request = fixture.Freeze<TestRequest>();
                var consumerMock = fixture.Freeze<Mock<IConsumer<TestRequest>>>();
                var i = 0;
                consumerMock.Setup(x => x.DequeueAsync())
                    .Callback(() => 
                    {
                        if (++i >= _numberOfItemsToDequeue) 
                        {
                            cts.Cancel();
                        }
                    })
                    .ReturnsAsync(() => request);
            }
        }
    }
}
