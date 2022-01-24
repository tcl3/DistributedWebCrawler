using Xunit;

namespace DistributedWebCrawler.Core.Tests.Collections
{
    [CollectionDefinition(nameof(SystemClockDependentCollection), DisableParallelization = true)]
    public class SystemClockDependentCollection
    {

    }

    [CollectionDefinition(nameof(RequestProcessorCollection), DisableParallelization = false)]
    public class RequestProcessorCollection
    {

    }
}
