using AutoFixture.Xunit2;
using DistributedWebCrawler.Core.Interfaces;
using DistributedWebCrawler.Core.Tests.Attributes;
using Moq;
using Xunit;

namespace DistributedWebCrawler.Core.Tests
{
    public class NodeStatusProviderTests
    {
        [Theory]
        [MoqAutoData]
        public void TotalBytesSentAndReceivedShouldDelegateToStreamManager(
            [Frozen] Mock<IStreamManager> streamManagerMock, 
            NodeStatusProvider sut)
        {
            streamManagerMock.Invocations.Clear();
            
            streamManagerMock.SetupGet(x => x.TotalBytesSent).Returns(2);
            streamManagerMock.SetupGet(x => x.TotalBytesReceived).Returns(3);
            
            var result = sut.CurrentNodeStatus;
            
            Assert.Equal(3, result.TotalBytesDownloaded);
            Assert.Equal(2, result.TotalBytesUploaded);

            streamManagerMock.VerifyGet(x => x.TotalBytesSent, Times.Once());
            streamManagerMock.VerifyGet(x => x.TotalBytesReceived, Times.Once());
        }

        [Theory]
        [MoqAutoData]
        public void NodeIdShouldPersistAcrossCalls(
            [Frozen] Mock<IStreamManager> streamManagerMock,
            NodeStatusProvider sut)
        {
            streamManagerMock.Invocations.Clear();

            var result1 = sut.CurrentNodeStatus;
            var result2 = sut.CurrentNodeStatus;

            Assert.Equal(result1.NodeId, result2.NodeId);
            streamManagerMock.VerifyGet(x => x.TotalBytesSent, Times.Exactly(2));
            streamManagerMock.VerifyGet(x => x.TotalBytesReceived, Times.Exactly(2));
        }
    }
}
