using DistributedWebCrawler.Core.Interfaces;
using DistributedWebCrawler.Core.Models;
using System;

namespace DistributedWebCrawler.Core
{
    public class NodeStatusProvider : INodeStatusProvider
    {
        // FIXME: This relies on DI to be unique on a given machine.
        // Need to of a better way of providing a per node unique ID.
        private readonly Guid _currentNodeId;
        private readonly IStreamManager _streamManager;

        public NodeStatusProvider(IStreamManager streamManager)
        {
            _currentNodeId = Guid.NewGuid();
            _streamManager = streamManager;
        }

        public NodeStatus CurrentNodeStatus => new(
            _currentNodeId, 
            totalBytesDownloaded: _streamManager.TotalBytesReceived, 
            totalBytesUploaded: _streamManager.TotalBytesSent
        );
    }
}
