using DistributedWebCrawler.Core.Enums;
using DistributedWebCrawler.Core.Extensions;
using DistributedWebCrawler.Core.Interfaces;
using DistributedWebCrawler.Core.Model;
using DistributedWebCrawler.Core.Queue;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DistributedWebCrawler.Core
{
    public class InMemoryCrawlerManager : ICrawlerManager
    {
        private readonly IEnumerable<ICrawlerComponent> _crawlerComponents;
        private readonly IEventReceiver<IngestRequest, IngestResult> _ingestEventReceiver;
        private readonly ISeeder _seeder;

        private bool _isSeeded;
        private bool _isStarted;

        private AsyncEventHandler<PageCrawlSuccess>? _onPageCrawlSuccess;
        private AsyncEventHandler<PageCrawlFailure>? _onPageCrawlFailure;

        public event AsyncEventHandler<PageCrawlSuccess>? OnPageCrawlSuccess
        {
            add
            {
                if (_onPageCrawlSuccess == null && _onPageCrawlFailure == null)
                {
                    OnIngestResultReceived += InMemoryCrawlerManager_OnIngestResultReceived;
                }

                _onPageCrawlSuccess += value;
            }
            remove
            {
                _onPageCrawlSuccess -= value;

                if (_onPageCrawlSuccess == null && _onPageCrawlFailure == null)
                {
                    OnIngestResultReceived -= InMemoryCrawlerManager_OnIngestResultReceived;
                }
            }
        }

        public event AsyncEventHandler<PageCrawlFailure>? OnPageCrawlFailure
        {
            add
            {
                if (_onPageCrawlSuccess == null && _onPageCrawlFailure == null)
                {
                    OnIngestResultReceived += InMemoryCrawlerManager_OnIngestResultReceived;
                }

                _onPageCrawlFailure += value;
            }
            remove
            {
                _onPageCrawlFailure -= value;

                if (_onPageCrawlSuccess == null && _onPageCrawlFailure == null)
                {
                    OnIngestResultReceived -= InMemoryCrawlerManager_OnIngestResultReceived;
                }
            }
        }


        private event ItemCompletedEventHandler<IngestResult> OnIngestResultReceived
        {
            add
            {
                _ingestEventReceiver.OnCompletedAsync += value;
            }
            remove
            {
                _ingestEventReceiver.OnCompletedAsync -= value;
            }
        }

        public InMemoryCrawlerManager(IEnumerable<ICrawlerComponent> crawlerComponents, IEventReceiver<IngestRequest, IngestResult> ingestEventReceiver, ISeeder seeder)
        {
            _crawlerComponents = crawlerComponents;
            _ingestEventReceiver = ingestEventReceiver;
            _seeder = seeder;
        }

        private Task InMemoryCrawlerManager_OnIngestResultReceived(object? sender, ItemCompletedEventArgs<IngestResult> e)
        {
            if (e.Status == TaskStatus.RanToCompletion && e.Result != null)
            {
                if (_onPageCrawlSuccess != null && e.Result.TryGetPageCrawlSuccess(out var pageCrawlSuccess)) 
                {
                    _onPageCrawlSuccess(this, pageCrawlSuccess);
                    return Task.CompletedTask;
                }

                if (_onPageCrawlFailure != null && e.Result.TryGetPageCrawlFailure(out var pageCrawlFailure))
                {
                    _onPageCrawlFailure(this, pageCrawlFailure);
                }
            }

            return Task.CompletedTask;
        }

        public async Task StartAsync(CrawlerStartState startState = CrawlerStartState.Running)
        {
            if (_isStarted)
            {
                throw new InvalidOperationException("Crawler already started");
            }

            _isStarted = true;

            if (!_isSeeded)
            {
                await _seeder.SeedAsync().ConfigureAwait(false);
                _isSeeded = true;
            }            

            await ForEachComponent(c => c.StartAsync(startState)).ConfigureAwait(false);
        }

        public Task PauseAsync()
        {
            return ForEachComponent(c => c.PauseAsync());
        }

        public Task ResumeAsync()
        {
            return ForEachComponent(c => c.ResumeAsync());
        }

        public Task WaitUntilCompletedAsync()
        {
            return ForEachComponent(c => c.WaitUntilCompletedAsync());
        }

        private Task ForEachComponent(Func<ICrawlerComponent, Task> componentAsyncAction)
        {
            var componentTasks = new List<Task>();
            foreach (var component in _crawlerComponents)
            {
                var componentTask = componentAsyncAction(component);
                componentTasks.Add(componentTask);
            }

            return Task.WhenAll(componentTasks.ToArray());
        }
    }
}
