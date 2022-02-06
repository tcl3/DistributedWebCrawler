using System;
using System.Threading;

namespace DistributedWebCrawler.Core.StreamManager
{
    public class StreamStats
    {
        public StreamStats()
        {
            StartedAt = DateTimeOffset.Now;
        }

        private long _totalBytesReceived;
        public long TotalBytesReceived => _totalBytesReceived;

        private long _totalBytesSent;
        public long TotalBytesSent => _totalBytesSent;

        public DateTimeOffset StartedAt { get; }

        public void UpdateBytesSent(int count)
        {
            Interlocked.Add(ref _totalBytesSent, count);
        }

        public void UpdateBytesReceived(int count)
        {
            Interlocked.Add(ref _totalBytesReceived, count);
        }
    }
}
