using DistributedWebCrawler.Core.Enums;
using DistributedWebCrawler.Core.Interfaces;

namespace DistributedWebCrawler.ComponentAPI
{
    public class ComponentBackgroundService : IHostedService
    {
        private readonly ICrawlerManager _crawlerManager;

        public ComponentBackgroundService(ICrawlerManager crawlerManager)
        {
            _crawlerManager = crawlerManager;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _crawlerManager.StartAsync(CrawlerRunningState.Paused).ConfigureAwait(false);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            // TODO: replace this with a hard stop, that passes in the cancellationToken
            return _crawlerManager.PauseAsync();
        }
    }
}
