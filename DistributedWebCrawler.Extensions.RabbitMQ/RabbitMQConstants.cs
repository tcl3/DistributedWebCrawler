namespace DistributedWebCrawler.Extensions.RabbitMQ
{
    internal static class RabbitMQConstants
    {
        internal static class CrawlerManager
        {
            internal const string ExchangeName = "CrawlerManager";
            internal const int RetryCount = 5;
        }

        internal static class ProducerConsumer
        {
            internal const string ExchangeName = "ProducerConsumer";
            internal const int RetryCount = 5;
        }
    }
}
