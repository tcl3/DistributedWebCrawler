using DistributedWebCrawler.Core.Enums;
using DistributedWebCrawler.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DistributedWebCrawler.Core
{
    public class InMemoryCrawlerManager : ICrawlerManager
    {
        private readonly IEnumerable<ICrawlerComponent> _crawlerComponents;
        private readonly ISeeder _seeder;

        private bool _isSeeded;
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
