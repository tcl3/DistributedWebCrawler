using DistributedWebCrawler.Core.Enums;
using DistributedWebCrawler.Core.Interfaces;
using DistributedWebCrawler.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DistributedWebCrawler.Core
{
    public class InMemoryCrawlerManager : ICrawlerManager
    {
        private readonly IEnumerable<ICrawlerComponent> _crawlerComponents;
        private readonly ISeeder _seeder;

        private bool _isStarted;

        public EventReceiverCollection Components { get; }

        public InMemoryCrawlerManager(IEnumerable<ICrawlerComponent> crawlerComponents, EventReceiverCollection eventReceivers, ISeeder seeder)
        {
            _crawlerComponents = crawlerComponents;
            Components = eventReceivers;
            _seeder = seeder;
        }       

        public async Task StartAsync(CrawlerRunningState startState = CrawlerRunningState.Running)
        {
            if (_isStarted)
            {
                throw new InvalidOperationException("Crawler already started");
            }

            _isStarted = true;

            await _seeder.SeedAsync().ConfigureAwait(false);

            await ForEachComponent(c => c.StartAsync(startState), ComponentFilter.MatchAll).ConfigureAwait(false);
        }

        public Task PauseAsync()
        {
            return PauseAsync(ComponentFilter.MatchAll);
        }

        public Task PauseAsync(ComponentFilter componentFilter)
        {
            return ForEachComponent(c => c.PauseAsync(), componentFilter);
        }

        public Task ResumeAsync()
        {
            return ResumeAsync(ComponentFilter.MatchAll);
        }

        public Task ResumeAsync(ComponentFilter componentFilter)
        {
            return ForEachComponent(c => c.ResumeAsync(), componentFilter);
        }

        public Task WaitUntilCompletedAsync()
        {
            return WaitUntilCompletedAsync(ComponentFilter.MatchAll);
        }

        public Task WaitUntilCompletedAsync(ComponentFilter componentFilter)
        {
            return ForEachComponent(c => c.WaitUntilCompletedAsync(), componentFilter);
        }

        private Task ForEachComponent(Func<ICrawlerComponent, Task> componentAsyncAction, ComponentFilter componentFilter)
        {
            var componentTasks = new List<Task>();
            foreach (var component in _crawlerComponents.Where(component => componentFilter.Matches(component)))
            {
                var componentTask = componentAsyncAction(component);
                componentTasks.Add(componentTask);
            }

            return Task.WhenAll(componentTasks.ToArray());
        }
    }
}
