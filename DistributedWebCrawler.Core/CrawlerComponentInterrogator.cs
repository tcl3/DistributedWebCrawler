using DistributedWebCrawler.Core.Enums;
using DistributedWebCrawler.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DistributedWebCrawler.Core
{
    public class CrawlerComponentInterrogator : ICrawlerComponentInterrogator
    {
        private IEnumerable<ICrawlerComponent> CrawlerComponents => _crawlerComponents.Value;

        private readonly Lazy<IEnumerable<ICrawlerComponent>> _crawlerComponents;

        // TODO: Figure out a way to pass IEnumerable<ICrawlerComponent> directly
        public CrawlerComponentInterrogator(Lazy<IEnumerable<ICrawlerComponent>> crawlerComponents)
        {
            _crawlerComponents = crawlerComponents;
        }

        public bool AllOtherComponentsAre(string requestingComponent, CrawlerComponentStatus status)
        {
            return CrawlerComponents
                .Where(x => x.Name != requestingComponent)
                .All(x => x.Status == status);
        }

        public CrawlerComponentStatus GetComponentStatus(string componentName)
        {
            var component = CrawlerComponents.Single(x => x.Name == componentName);
            return component.Status;
        }

        public CrawlerComponentStatus GetComponentStatus<TComponent>() 
            where TComponent : ICrawlerComponent
        {
            var component = CrawlerComponents.Single(x => x.GetType() == typeof(TComponent));
            return component.Status;
        }
    }
}
