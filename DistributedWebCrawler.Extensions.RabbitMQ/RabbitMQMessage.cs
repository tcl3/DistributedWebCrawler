namespace DistributedWebCrawler.Extensions.RabbitMQ
{
    internal class RabbitMQMessage<TContent>
    {
        public RabbitMQMessage(Guid nodeId, TContent messageContent)
        {
            NodeId = nodeId;
            MessageContent = messageContent;
        }

        public Guid NodeId { get; }
        public TContent MessageContent { get; }
    }
}
