using DistributedWebCrawler.Core.Attributes;
using DistributedWebCrawler.Core.Components;
using DistributedWebCrawler.Core.Interfaces;
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
        DefaultValue = 0,
    }

    [ComponentName("Test", typeof(TestSuccess), typeof(ErrorCode<TestFailure>))]
    public class TestComponent : IRequestProcessor<TestRequest>
    {

        public Task<QueuedItemResult> ProcessItemAsync(TestRequest item, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
