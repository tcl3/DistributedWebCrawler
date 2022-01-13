using DistributedWebCrawler.Core.Attributes;
using DistributedWebCrawler.Core.Components;
using DistributedWebCrawler.Core.Model;
using System;
using System.Threading;
using System.Threading.Tasks;

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

    }

    [ComponentName("Test")]
    public class TestComponentWithNameAttribute : TestComponent
    {
    }

    public class TestComponent
        : AbstractTaskQueueComponent<TestRequest, TestSuccess, ErrorCode<TestFailure>>
    {
        public TestComponent()
            : base(null!, null!, null!, null!, null!, null!)
        {
        }

        protected override Task<QueuedItemResult> ProcessItemAsync(TestRequest item, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
