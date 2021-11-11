using DistributedWebCrawler.Core.Enums;

namespace DistributedWebCrawler.Core.Interfaces
{
    public interface ICrawlerComponentInterrogator
    {
        CrawlerComponentStatus GetComponentStatus(string componentName);
        CrawlerComponentStatus GetComponentStatus<TComponent>() where TComponent : ICrawlerComponent;

        bool AllOtherComponentsAre(string requestingComponent, CrawlerComponentStatus status);

    }
}
