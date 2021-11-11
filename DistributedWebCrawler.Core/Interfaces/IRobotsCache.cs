using System;
using System.Threading.Tasks;

namespace DistributedWebCrawler.Core.Interfaces
{
    public interface IRobotsCache
    {
        Task GetRobotsForHostAsync(Uri host, Action<IRobots> ifExistsAction);
    }
}
