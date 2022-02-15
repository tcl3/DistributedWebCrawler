using DistributedWebCrawler.Core.Attributes;
using DistributedWebCrawler.Core.Models;
using DistributedWebCrawler.Core.Configuration;
using DistributedWebCrawler.Core.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using DistributedWebCrawler.Core.Components;

namespace DistributedWebCrawler.Core.Tests.Fakes
{
    internal interface TestComponentMarkerInterface
    {

    }

    public class TestSuccess
    {

    }

    public enum TestFailure
    {
        DefaultValue = 0,
    }

    [Component("Test", typeof(TestSuccess), typeof(ErrorCode<TestFailure>))]
    public class TestRequestProcessor : IRequestProcessor<TestRequest>
    {

        public Task<QueuedItemResult> ProcessItemAsync(TestRequest item, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }

    public class TestComponent : TaskQueueComponent<TestRequest, TestSuccess, ErrorCode<TestFailure>, TestSettings>
    {
        public TestComponent(IRequestProcessor<TestRequest> requestProcessor,
            IConsumer<TestRequest> consumer, 
            IEventDispatcher<TestSuccess, ErrorCode<TestFailure>> eventReceiver, 
            IKeyValueStore keyValueStore, 
            ILogger<TaskQueueComponent<TestRequest, TestSuccess, ErrorCode<TestFailure>, TestSettings>> logger, 
            IComponentNameProvider componentNameProvider, 
            INodeStatusProvider nodeStatusProvider,
            TestSettings settings) 
            : base(requestProcessor, consumer, eventReceiver, keyValueStore, logger, 
                  componentNameProvider, nodeStatusProvider, settings)
        {
        }
    }

    public class TestSettings : TaskQueueSettings
    {

    }

}
