using DistributedWebCrawler.Core.Interfaces;
using System;

namespace DistributedWebCrawler.Core.Extensions
{
    public static class QueueExtensions
    {
        public static bool IsEmpty(this IProducerConsumer queue)
        {
            if (queue == null)
            {
                throw new ArgumentNullException(nameof(queue));
            }

            return queue.Count == 0;
        }
    }
}
