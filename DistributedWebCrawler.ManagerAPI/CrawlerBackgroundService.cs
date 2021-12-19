using DistributedWebCrawler.Core.Components;
using DistributedWebCrawler.Core.Enums;
using DistributedWebCrawler.Core.Interfaces;
using DistributedWebCrawler.Core.Queue;
using DistributedWebCrawler.ManagerAPI.Hubs;

namespace DistributedWebCrawler.ManagerAPI
{
    public class CrawlerBackgroundService : IHostedService
    {
        private readonly ICrawlerManager _crawlerManager;
        private readonly ComponentHubEventListener _eventListener;

        public CrawlerBackgroundService(ICrawlerManager crawlerManager, 
            ComponentHubEventListener eventListener)
        {
            _crawlerManager = crawlerManager;
            _eventListener = eventListener;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _crawlerManager.StartAsync(CrawlerRunningState.Running).ConfigureAwait(false);

            _eventListener.Register(_crawlerManager.Components.All);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            // TODO: replace this with a hard stop, that passes in the cancellationToken
            return _crawlerManager.PauseAsync();
        }
    }
}
