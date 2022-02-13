using DistributedWebCrawler.Core.Interfaces;
using DistributedWebCrawler.Core.Models;
using Microsoft.AspNetCore.SignalR;

namespace DistributedWebCrawler.ManagerAPI.Hubs
{
    public class CrawlerHub : Hub<IComponentEventsHub>, ICommandHub
    {
        private readonly ICrawlerManager _crawlerManager;

        public CrawlerHub(ICrawlerManager crawlerManager)
        {
            _crawlerManager = crawlerManager;
        }

        public Task Pause(ComponentFilter? componentFilter = null)
        {
            componentFilter ??= ComponentFilter.MatchAll;
            return _crawlerManager.PauseAsync(componentFilter);
        }

        public async Task Resume(ComponentFilter? componentFilter = null)
        {
            componentFilter ??= ComponentFilter.MatchAll;
            await _crawlerManager.ResumeAsync(componentFilter);
        }
    }
}
