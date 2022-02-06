using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace DistributedWebCrawler.Core.Interfaces
{
    public interface IStreamManager
    {
        long TotalBytesSent { get; }
        long TotalBytesReceived { get; }
        int ActiveSockets { get; }
        DateTimeOffset StartedAt { get; }
        ValueTask<Stream> ConnectCallback(SocketsHttpConnectionContext context, CancellationToken cancellationToken);
    }
}
