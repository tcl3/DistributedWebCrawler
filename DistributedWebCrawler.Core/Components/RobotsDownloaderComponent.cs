using DistributedWebCrawler.Core.Attributes;
using DistributedWebCrawler.Core.Configuration;
using DistributedWebCrawler.Core.Extensions;
using DistributedWebCrawler.Core.Interfaces;
using DistributedWebCrawler.Core.Model;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace DistributedWebCrawler.Core.Components
{
    public class RobotsDownloaderSuccess 
    {
        public RobotsDownloaderSuccess(Uri uri)
        {
            Uri = uri;
        }
        public Uri Uri { get; init; }
    }

    public enum RobotsDownloaderFailure
    {

    }

    [ComponentName("RobotsDownloader")]    
    public class RobotsDownloaderComponent : AbstractTaskQueueComponent<RobotsRequest, RobotsDownloaderSuccess, ErrorCode<RobotsDownloaderFailure>>
    {
        private readonly ILogger<RobotsDownloaderComponent> _logger;
        private readonly IProducer<SchedulerRequest> _schedulerRequestProducer;
        private readonly IRobotsCacheWriter _robotsCache;

        private readonly TimeSpan _expirationTimeSpan;

        public RobotsDownloaderComponent(IConsumer<RobotsRequest> consumer,
            IEventDispatcher<RobotsDownloaderSuccess, ErrorCode<RobotsDownloaderFailure>> eventDispatcher,
            IKeyValueStore keyValueStore,
            ILogger<RobotsDownloaderComponent> logger,
            ComponentNameProvider componentNameProvider,
            IProducer<SchedulerRequest> schedulerRequestProducer,
            IRobotsCacheWriter robotsCacheWriter,
            RobotsTxtSettings settings) 
            : base(consumer, eventDispatcher, keyValueStore, logger, componentNameProvider, settings)
        {
            _logger = logger;
            _schedulerRequestProducer = schedulerRequestProducer;
            _robotsCache = robotsCacheWriter;

            _expirationTimeSpan = TimeSpan.FromSeconds(settings.CacheIntervalSeconds);
        }

        protected override async Task<QueuedItemResult> ProcessItemAsync(RobotsRequest item, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Processing robots.txt request for {item.Uri}");

            await _robotsCache.AddOrUpdateRobotsForHostAsync(item.Uri, _expirationTimeSpan, cancellationToken).ConfigureAwait(false);

            await RequeueAsync(item.SchedulerRequestId, _schedulerRequestProducer, cancellationToken).ConfigureAwait(false);

            return Success(item, new RobotsDownloaderSuccess(item.Uri));
        }
    }
}
