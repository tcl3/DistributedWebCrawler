using DistributedWebCrawler.Core.Interfaces;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace DistributedWebCrawler.Core
{
    public class StreamManager : IStreamManager
    {
        private readonly ConcurrentDictionary<ByteCountingStream, bool> _streamLookup;
        
        private readonly IDnsResolver? _customDnsResolver;

        private long _totalBytesSent;
        public long TotalBytesSent => _totalBytesSent;

        private long _totalBytesReceived;
        public long TotalBytesReceived => _totalBytesReceived;

        public int ActiveSockets => _streamLookup.Count;

        public DateTimeOffset StartedAt { get; }

        public StreamManager(IDnsResolver? customDnsResolver = null)
        {
            _streamLookup = new();
            _customDnsResolver = customDnsResolver;

            StartedAt = DateTimeOffset.Now;
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

                var wrappedStream = new ByteCountingStream(stream)
                {
                    DisposeCallback = stream => DisposeStream(stream),
                    UpdateBytesReceivedCallback = UpdateBytesReceived,
                    UpdateBytesSentCallback = UpdateBytesSent
                };

                _streamLookup.TryAdd(wrappedStream, true);
                return wrappedStream;
            }
            catch
            {
                socket?.Dispose();
                throw;
            }
        }

        private void UpdateBytesReceived(int count)
        {
            Interlocked.Add(ref _totalBytesReceived, count);
        }

        private void UpdateBytesSent(int count)
        {
            Interlocked.Add(ref _totalBytesSent, count);
        }

        private void DisposeStream(ByteCountingStream stream)
        {
            _streamLookup.TryRemove(stream, out _);
        }
    }
}
