using DistributedWebCrawler.Core.Models;
using DistributedWebCrawler.Core.Tests.Attributes;
using DistributedWebCrawler.Core.Tests.Fakes;
using FluentAssertions;
using System.Text;
using Xunit;

namespace DistributedWebCrawler.Core.Tests.Serialization
{
    public class ErrorCodeSerializationTests
        : JsonSerializerAdaptorTestBase<ErrorCode<TestFailure>>
    {
    }

    public class SchedulerRequestSerializationTests 
        : JsonSerializerAdaptorTestBase<SchedulerRequest>
    {
    }

    public class IngestRequestSerializationTests
        : JsonSerializerAdaptorTestBase<IngestRequest>
    {
    }

    public class IngestSuccessSerializationTests
        : JsonSerializerAdaptorTestBase<IngestSuccess>
    {
    }

    public class IngestFailureSerializationTests
    : JsonSerializerAdaptorTestBase<IngestFailure>
    {
    }

    public class ParseRequestSerializationTests
        : JsonSerializerAdaptorTestBase<ParseRequest>
    {
    }

    public class RobotsRequestSerializationTests
        : JsonSerializerAdaptorTestBase<RobotsRequest>
    {
    }

    public class ComponentStatusSerializationTests
        : JsonSerializerAdaptorTestBase<ComponentStatus>
    {
    }

    public abstract class JsonSerializerAdaptorTestBase<TData>
    {
        [Theory]
        [SerializerOptionsAutoData]
        public void EnsureBytesCanBeDeserialized(TData data, JsonSerializerAdaptor sut)
        {
            var serializedDataBytes = sut.Serialize(data);
            var deserializedData = sut.Deserialize<TData>(serializedDataBytes);
            
            deserializedData.Should().BeEquivalentTo(data);
        }

        [Theory]
        [SerializerOptionsAutoData]
        public void EnsureStringCanBeDeserialized(TData data, JsonSerializerAdaptor sut)
        {
            var serializedDataBytes = sut.Serialize(data);
            var serializedDataString = Encoding.UTF8.GetString(serializedDataBytes);
            var deserializedData = sut.Deserialize<TData>(serializedDataString);
            
            deserializedData.Should().BeEquivalentTo(data);
        }
    }
}
