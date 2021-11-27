﻿using DistributedWebCrawler.Core.Configuration;
using DistributedWebCrawler.Core.Interfaces;
using DistributedWebCrawler.Core.Model;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace DistributedWebCrawler.Core.Components
{
    public class RobotsDownloaderComponent : AbstractTaskQueueComponent<RobotsRequest>
    {
        private readonly ILogger<RobotsDownloaderComponent> _logger;
        private readonly IProducer<SchedulerRequest, bool> _schedulerRequestProducer;
        private readonly IRobotsCacheWriter _robotsCache;

        private readonly TimeSpan _expirationTimeSpan;

        public RobotsDownloaderComponent(IConsumer<RobotsRequest, bool> consumer,
            ILogger<RobotsDownloaderComponent> logger,
            IProducer<SchedulerRequest, bool> schedulerRequestProducer,
            IRobotsCacheWriter robotsCacheWriter,
            RobotsTxtSettings settings) : base(consumer, logger, nameof(RobotsDownloaderComponent), settings)
        {
            _logger = logger;
            _schedulerRequestProducer = schedulerRequestProducer;
            _robotsCache = robotsCacheWriter;

            _expirationTimeSpan = TimeSpan.FromSeconds(settings.CacheIntervalSeconds);
        }

        protected override async Task<bool> ProcessItemAsync(RobotsRequest item, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Processing robots.txt request for {item.Uri}");

            await _robotsCache.AddOrUpdateRobotsForHostAsync(item.Uri, _expirationTimeSpan, cancellationToken).ConfigureAwait(false); ;
            
            _schedulerRequestProducer.Enqueue(item.SchedulerRequest);

            return true;
        }
    }
}
