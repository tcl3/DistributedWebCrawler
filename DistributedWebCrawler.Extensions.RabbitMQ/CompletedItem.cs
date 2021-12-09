namespace DistributedWebCrawler.Extensions.RabbitMQ
{
    internal class CompletedItem<TResult>
    {
        public CompletedItem(Guid id, TResult result)
        {
            Id = id;
            Result = result;
        }

        public TResult Result { get; init; }
        public Guid Id { get; init; }
    }
}
