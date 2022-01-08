using System;
using System.Threading;
using System.Threading.Tasks;

namespace DistributedWebCrawler.Core.Interfaces
{
    public interface IRobotsCacheWriter
    {
        Task<string> AddOrUpdateRobotsForHostAsync(Uri host, TimeSpan expirationTimeSpan, CancellationToken cancellationToken);
    }

    public interface IRobotsCacheReader
    {
        Task<bool> GetRobotsTxtAsync(Uri uri, Action<IRobots>? ifExistsAction, CancellationToken cancellationToken);
    }
}
