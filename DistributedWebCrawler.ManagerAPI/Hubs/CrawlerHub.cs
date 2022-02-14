using DistributedWebCrawler.Core.Interfaces;
using DistributedWebCrawler.Core.Models;
using DistributedWebCrawler.ManagerAPI.Models;
using Microsoft.AspNetCore.SignalR;

namespace DistributedWebCrawler.ManagerAPI.Hubs
{
    public class CrawlerHub : Hub<IComponentEventsHub>, ICommandHub
    {
        private readonly ICrawlerManager _crawlerManager;
        private readonly ComponentHubEventListener _componentHubEventListener;

        public CrawlerHub(ICrawlerManager crawlerManager, ComponentHubEventListener componentHubEventListener)
        {
            _crawlerManager = crawlerManager;
            _componentHubEventListener = componentHubEventListener;
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

        public Task UpdateComponentStats(Guid componentId)
        {
            _componentHubEventListener.UpdateComponentStats(Context.ConnectionId, componentId);
            return Task.CompletedTask;
        }

        public Task UpdateAllComponentStats()
        {
            _componentHubEventListener.UpdateComponentStats(Context.ConnectionId);
            return Task.CompletedTask;
        }
    }
}
