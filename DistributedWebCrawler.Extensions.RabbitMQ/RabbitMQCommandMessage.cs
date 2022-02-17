using DistributedWebCrawler.Core;
using DistributedWebCrawler.Core.Models;

namespace DistributedWebCrawler.Extensions.RabbitMQ
{
    internal class RabbitMQCommandMessage
    {
        public Command Command { get;  }
        public ComponentFilter? ComponentFilter { get; }

        public RabbitMQCommandMessage(Command command, ComponentFilter componentFilter)
        {
            Command = command;
            ComponentFilter = componentFilter == ComponentFilter.MatchAll 
                ? null 
                : componentFilter;
        }
    }
}
