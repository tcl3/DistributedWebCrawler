using DistributedWebCrawler.Core.Interfaces;

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
            await _crawlerManager.StartAsync();
            
            // Start the crawler in the paused state
            // TODO: Add a call to the CrawlerManager to initialize the crawler without starting it.
            await _crawlerManager.PauseAsync();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            // TODO: replace this with a hard stop, that passes in the cancellationToken
            return _crawlerManager.PauseAsync();
        }
    }
}
