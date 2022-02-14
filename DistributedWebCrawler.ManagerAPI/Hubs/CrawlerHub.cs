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

        public Task Pause()
        {
            return _crawlerManager.PauseAsync();
        }

        public async Task Resume()
        {
            await _crawlerManager.ResumeAsync();
        }

        public Task PauseComponent(ComponentFilter componentFilter)
        {
            return _crawlerManager.PauseAsync(componentFilter);
        }

        public async Task ResumeComponent(ComponentFilter componentFilter)
        {
            await _crawlerManager.ResumeAsync(componentFilter);
        }
    }
}
