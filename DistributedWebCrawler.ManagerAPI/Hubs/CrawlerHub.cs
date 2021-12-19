using DistributedWebCrawler.Core.Interfaces;
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
    }
}
