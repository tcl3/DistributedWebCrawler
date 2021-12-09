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
        private readonly ComponentEventsHub _hub;

        public CrawlerBackgroundService(ICrawlerManager crawlerManager, ComponentEventsHub hub)
        {
            _crawlerManager = crawlerManager;
            _hub = hub;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _crawlerManager.StartAsync(CrawlerStartState.Running).ConfigureAwait(false);

            _crawlerManager.Components.All.OnComponentUpdateAsync += OnComponentUpdateAsync;
            _crawlerManager.Components.All.OnCompletedAsync += OnCompletedAsync;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            // TODO: replace this with a hard stop, that passes in the cancellationToken
            return _crawlerManager.PauseAsync();
        }

        private async Task OnCompletedAsync(object? sender, ItemCompletedEventArgs e)
        {
            await _hub.Clients.All.OnCompleted(e).ConfigureAwait(false);
        }

        private async Task OnComponentUpdateAsync(object? sender, ComponentEventArgs<ComponentStatus> e)
        {
            await _hub.Clients.All.OnComponentUpdate(e).ConfigureAwait(false);
        }
    }
}
