using DistributedWebCrawler.Core.Models;

namespace DistributedWebCrawler.Core.Interfaces
{
    public interface INodeStatusProvider
    {
        NodeStatus CurrentNodeStatus { get; }
    }
}
