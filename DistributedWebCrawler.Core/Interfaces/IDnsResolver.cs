using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace DistributedWebCrawler.Core.Interfaces
{
    public interface IDnsResolver
    {
        ValueTask<IPEndPoint> ResolveAsync(DnsEndPoint dnsEndPoint, CancellationToken cancellationToken = default);
    }
}
