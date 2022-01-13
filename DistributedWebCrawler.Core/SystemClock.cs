using System;
using System.Threading;
using System.Threading.Tasks;

namespace DistributedWebCrawler.Core
{
    public static class SystemClock
    {
        public static Func<DateTime> Now { get; set; } = () => DateTime.Now;
        public static Func<DateTime> UtcNow { get; set; } = () => DateTime.UtcNow;
        public static Func<DateTimeOffset> DateTimeOffsetNow { get; set; } = () => DateTimeOffset.UtcNow;
        public static Func<TimeSpan, CancellationToken, Task> DelayAsync { get; set; } = Task.Delay;


        public static void Reset()
        {
            Now = () => DateTime.Now;
            UtcNow = () => DateTime.UtcNow;
            DateTimeOffsetNow = () => DateTimeOffset.Now;
            DelayAsync = Task.Delay;
        }
    }
}
