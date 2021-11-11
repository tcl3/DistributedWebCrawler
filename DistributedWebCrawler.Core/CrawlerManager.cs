﻿using DistributedWebCrawler.Core.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DistributedWebCrawler.Core
{
    public class CrawlerManager : ICrawlerManager
    {
        private readonly IEnumerable<ICrawlerComponent> _crawlerComponents;
        private readonly ISeeder _seeder;
        private readonly ILogger<CrawlerManager> _logger;

        private bool _isSeeded;
        private bool _isStarted;
        public CrawlerManager(IEnumerable<ICrawlerComponent> crawlerComponents,
            ISeeder seeder, ILogger<CrawlerManager> logger)
        {
            _crawlerComponents = crawlerComponents;
            
            _seeder = seeder;
            _logger = logger;
        }

        public async Task StartAsync()
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

            await ForEachComponent(c => c.StartAsync()).ConfigureAwait(false);
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
