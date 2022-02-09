using DistributedWebCrawler.Core.Attributes;
using DistributedWebCrawler.Core.Configuration;
using DistributedWebCrawler.Core.Extensions;
using DistributedWebCrawler.Core.Interfaces;
using DistributedWebCrawler.Core.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace DistributedWebCrawler.Core.RequestProcessors
{

    [Component(name: "RobotsDownloader", successType: typeof(RobotsDownloaderSuccess), failureType: typeof(ErrorCode<RobotsDownloaderFailure>))]
    public class RobotsDownloaderRequestProcessor : IRequestProcessor<RobotsRequest>
    {
        private readonly ILogger<RobotsDownloaderRequestProcessor> _logger;
        private readonly IProducer<SchedulerRequest> _schedulerRequestProducer;
        private readonly IRobotsCacheWriter _robotsCache;
        private readonly IKeyValueStore _outstandingItemsStore;

        private readonly TimeSpan _expirationTimeSpan;

        public RobotsDownloaderRequestProcessor(
            ILogger<RobotsDownloaderRequestProcessor> logger,
            IProducer<SchedulerRequest> schedulerRequestProducer,
            IKeyValueStore keyValueStore,
            IRobotsCacheWriter robotsCacheWriter,
            RobotsTxtSettings settings)
        {
            _logger = logger;
            _schedulerRequestProducer = schedulerRequestProducer;
            _outstandingItemsStore = keyValueStore.WithKeyPrefix("TaskQueueOutstandingItems");
            _robotsCache = robotsCacheWriter;

            _expirationTimeSpan = TimeSpan.FromSeconds(settings.CacheIntervalSeconds);
        }

        public async Task<QueuedItemResult> ProcessItemAsync(RobotsRequest item, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation($"Processing robots.txt request for {item.Uri}");

            var content = await _robotsCache.AddOrUpdateRobotsForHostAsync(item.Uri, _expirationTimeSpan, cancellationToken).ConfigureAwait(false);

            await _schedulerRequestProducer.RequeueAsync(item.SchedulerRequestId, _outstandingItemsStore, cancellationToken).ConfigureAwait(false);

            var result = new RobotsDownloaderSuccess(item.Uri)
            {
                ContentLength = content.Length
            };

            return item.Success(result);
        }
    }
}
