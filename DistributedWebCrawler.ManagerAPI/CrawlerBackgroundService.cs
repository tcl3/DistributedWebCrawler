using DistributedWebCrawler.Core.Components;
using DistributedWebCrawler.Core.Enums;
using DistributedWebCrawler.Core.Interfaces;
using DistributedWebCrawler.Core.Queue;

namespace DistributedWebCrawler.ManagerAPI
{
    public class CrawlerBackgroundService : IHostedService
    {
        private readonly ICrawlerManager _crawlerManager;

        public CrawlerBackgroundService(ICrawlerManager crawlerManager)
        {
            _crawlerManager = crawlerManager;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _crawlerManager.StartAsync(CrawlerRunningState.Running).ConfigureAwait(false);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            // TODO: replace this with a hard stop, that passes in the cancellationToken
            return _crawlerManager.PauseAsync();
        }
    }
}
