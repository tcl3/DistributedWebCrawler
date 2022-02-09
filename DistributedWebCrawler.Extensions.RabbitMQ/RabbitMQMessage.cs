namespace DistributedWebCrawler.Extensions.RabbitMQ
{
    internal class RabbitMQMessage<TContent>
    {
        public RabbitMQMessage(Guid componentId, Guid nodeId, TContent messageContent)
        {
            ComponentId = componentId;
            NodeId = nodeId;
            MessageContent = messageContent;
        }

        public Guid ComponentId { get; }
        public Guid NodeId { get; }
        public TContent MessageContent { get; }
    }
}
