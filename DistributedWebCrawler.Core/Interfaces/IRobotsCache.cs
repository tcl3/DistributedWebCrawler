using System;
using System.Threading;
using System.Threading.Tasks;

namespace DistributedWebCrawler.Core.Interfaces
{
    public interface IRobotsCache
    {
        Task<bool> GetRobotsTxtAsync(Uri uri, Action<IRobots> ifExistsAction, CancellationToken cancellationToken);
        Task AddOrUpdateRobotsForHostAsync(Uri host, CancellationToken cancellationToken);
    }
}
