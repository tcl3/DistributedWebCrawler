using DistributedWebCrawler.Core.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace DistributedWebCrawler.Core.StreamManager
{
    public class StreamManager : IStreamManager
    {
        private readonly ConcurrentDictionary<ByteCountingStream, bool> _streamLookup;
        private readonly StreamStats _streamStats;
        private readonly IDnsResolver? _customDnsResolver;

        public long TotalBytesSent => _streamStats.TotalBytesSent;
        public long TotalBytesReceived => _streamStats.TotalBytesReceived;
        public int ActiveSockets => _streamLookup.Count;

        public DateTimeOffset StartedAt => _streamStats.StartedAt;

        public StreamManager(IDnsResolver? customDnsResolver = null)
        {
            _streamLookup = new();
            _streamStats = new();
            _customDnsResolver = customDnsResolver;
        }


        public async ValueTask<Stream> ConnectCallback(SocketsHttpConnectionContext context, CancellationToken cancellationToken)
        {
            var socket = new Socket(SocketType.Stream, ProtocolType.Tcp)
            {
                NoDelay = true,
            };
            try
            {
                EndPoint endPoint = context.DnsEndPoint;
                if (_customDnsResolver != null)
                {
                    endPoint = await _customDnsResolver.ResolveAsync(context.DnsEndPoint, cancellationToken).ConfigureAwait(false);
                }

                await socket.ConnectAsync(endPoint, cancellationToken).ConfigureAwait(false);
                var stream = new NetworkStream(socket, ownsSocket: true);

                var wrappedStream = new ByteCountingStream(this, _streamStats, stream);
                _streamLookup.TryAdd(wrappedStream, true);
                return wrappedStream;
            }
            catch
            {
                socket?.Dispose();
                throw;
            }
        }

        internal void DisposeStream(ByteCountingStream stream)
        {
            _streamLookup.TryRemove(stream, out _);
        }
    }
}
