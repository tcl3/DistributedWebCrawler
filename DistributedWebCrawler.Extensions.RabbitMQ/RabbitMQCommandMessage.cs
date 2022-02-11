using DistributedWebCrawler.Core;

namespace DistributedWebCrawler.Extensions.RabbitMQ
{
    internal class RabbitMQCommandMessage
    {
        public Command Command { get; init; }
        public ComponentFilter ComponentFilter { get; init; }

        public RabbitMQCommandMessage(Command command, ComponentFilter componentFilter)
        {
            Command = command;
            ComponentFilter = componentFilter;
        }
    }
}
