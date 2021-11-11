using DistributedWebCrawler.Core.Configuration;
using DistributedWebCrawler.Core.Enums;
using DistributedWebCrawler.Core.Interfaces;
using DistributedWebCrawler.Core.Model;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Priority_Queue;

namespace DistributedWebCrawler.Core.Components
{
    public class SchedulerCrawlerComponent : AbstractQueuedCrawlerComponent<SchedulerRequest>
    {
        private class SchedulerQueueEntry
        {
            public Uri Uri { get; set; }
            public SchedulerRequest SchedulerRequest { get; set; }

            public SchedulerQueueEntry(Uri uri, SchedulerRequest schedulerRequest)
            {
                Uri = uri;
                SchedulerRequest = schedulerRequest;
            }
        }

        private readonly SchedulerSettings _schedulerSettings;
        private readonly ILogger<SchedulerCrawlerComponent> _logger;
        private readonly IRobotsCache _robotsCache;
        private readonly IProducer<IngestRequest> _ingestRequestProducer;

        private readonly ConcurrentDictionary<Uri, bool> _visitedUris;
        private readonly ConcurrentDictionary<string, IEnumerable<string>> _visitedPathsLookup;
        private readonly SimplePriorityQueue<SchedulerQueueEntry, DateTimeOffset> _nextPathForHostQueue;

        public SchedulerCrawlerComponent(SchedulerSettings schedulerSettings,
            IConsumer<SchedulerRequest> consumer,
            ILogger<SchedulerCrawlerComponent> logger,
            IRobotsCache robotsCache,
            IProducer<IngestRequest> ingestRequestProducer)
            : base(consumer, logger, nameof(SchedulerCrawlerComponent), schedulerSettings.MaxConcurrentRobotsRequests)
        {
            _schedulerSettings = schedulerSettings;
            _logger = logger;
            _robotsCache = robotsCache;
            _ingestRequestProducer = ingestRequestProducer;

            _visitedUris = new();
            _visitedPathsLookup = new();
            _nextPathForHostQueue = new();
        }

        protected override Task ComponentStartAsync()
        {
            var queueLoopTask = base.ComponentStartAsync();
            var schedulerLoopTask = Task.Run(SchedulerLoop);
            return Task.WhenAll(new[] { queueLoopTask, schedulerLoopTask });
        }

        protected override CrawlerComponentStatus GetStatus()
        {
            // TODO: Implement the correct status here to allow us to exit when done
            return CrawlerComponentStatus.Busy;
        }

        private async Task SchedulerLoop()
        {
            while (Status != CrawlerComponentStatus.Completed)
            {
                while (_nextPathForHostQueue.TryFirst(out var entry) && _nextPathForHostQueue.TryGetPriority(entry, out var notBefore) && notBefore <= DateTimeOffset.Now)
                {
                    _ = _nextPathForHostQueue.Dequeue();

                    var schedulerRequest = entry.SchedulerRequest;

                    if (!_visitedUris.ContainsKey(entry.Uri))
                    {
                        _logger.LogDebug($"Enqueueing URI: {entry.Uri} for ingestion");

                        var ingestRequest = new IngestRequest(entry.Uri)
                        {
                            CurrentCrawlDepth = schedulerRequest.CurrentCrawlDepth,
                            MaxDepthReached = schedulerRequest.CurrentCrawlDepth >= _schedulerSettings.MaxCrawlDepth
                        };

                        _ingestRequestProducer.Enqueue(ingestRequest);
                        _visitedUris.AddOrUpdate(entry.Uri, true, (key, oldValue) => oldValue);
                    }

                    AddNextUriToSchedulerQueue(schedulerRequest);
                }

                // FIXME: This is currently here to avoid hammering the CPU. We should probably use a mutex/semaphore with an appropriate timeout.
                
                await Task.Delay(1).ConfigureAwait(false);
            }
        }

        protected async override Task ProcessItemAsync(SchedulerRequest schedulerRequest)
        {
            if (schedulerRequest.CurrentCrawlDepth > _schedulerSettings.MaxCrawlDepth)
            {
                _logger.LogError($"Not processing {schedulerRequest.Uri}. Maximum crawl depth exceeded (curremt: {schedulerRequest.CurrentCrawlDepth}, max: {_schedulerSettings.MaxCrawlDepth})");
                return;
            }

            var visitedPathsForHost = Enumerable.Empty<string>();
            _visitedPathsLookup.AddOrUpdate(schedulerRequest.Uri.ToString(), schedulerRequest.Paths,
                (key, oldValue) =>
                {
                    visitedPathsForHost = oldValue;
                    return oldValue.Union(schedulerRequest.Paths);
                });

            var pathsToVisit = schedulerRequest.Paths.Except(visitedPathsForHost);

            if (pathsToVisit.Any() && (_schedulerSettings.RespectsRobotsTxt ?? false))
            {
                await _robotsCache.GetRobotsForHostAsync(schedulerRequest.Uri, robots => 
                {
                    pathsToVisit = pathsToVisit.Where(path =>
                    {
                        var allowed = robots.Allowed(path);

                        if (!allowed)
                        {
                            _logger.LogDebug($"Path {path} disallowed for host {schedulerRequest.Uri} by robots.txt");
                        }

                        return allowed;
                    });
                }).ConfigureAwait(false);
            }

            if (!pathsToVisit.Any())
            {
                _logger.LogDebug($"Not processing request for host: {schedulerRequest.Uri}. No unvisited paths");
                return;
            }

            schedulerRequest.Paths = pathsToVisit;

            if (!_nextPathForHostQueue.Any(x => x.Uri.Host == schedulerRequest.Uri.Host))
            {
                AddNextUriToSchedulerQueue(schedulerRequest, first: true);
            }
        }

        private void AddNextUriToSchedulerQueue(SchedulerRequest schedulerRequest, bool first = false)
        {
            var nextPathForHost = schedulerRequest.Paths.First();
            var pathsToQueue = schedulerRequest.Paths;

            if (!first)
            {
                pathsToQueue = pathsToQueue.Skip(1);
            }

            schedulerRequest.Paths = pathsToQueue;

            if (schedulerRequest.Paths.Any())
            {
                var queueEntry = new SchedulerQueueEntry(new Uri(schedulerRequest.Uri, nextPathForHost), schedulerRequest);
                var notBefore = DateTimeOffset.Now;
                if (!first)
                {
                    notBefore = notBefore.AddMilliseconds(_schedulerSettings.SameDomainCrawlDelayMillis);
                }

                _nextPathForHostQueue.Enqueue(queueEntry, notBefore);

                _visitedPathsLookup.AddOrUpdate(schedulerRequest.Uri.ToString(), pathsToQueue, (key, oldValue) => oldValue.Union(schedulerRequest.Paths).ToHashSet());
            }
        }
    }
}
