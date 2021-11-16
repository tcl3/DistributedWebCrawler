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
using Nager.PublicSuffix;

namespace DistributedWebCrawler.Core.Components
{
    public class SchedulerComponent : AbstractTaskQueueComponent<SchedulerRequest>
    {
        private class SchedulerQueueEntry
        {
            public Uri Uri { get;  }
            public string Domain { get; }
            public SchedulerRequest SchedulerRequest { get; }

            public SchedulerQueueEntry(Uri uri, string domain, SchedulerRequest schedulerRequest)
            {
                Uri = uri;
                Domain = domain;
                SchedulerRequest = schedulerRequest;
            }
        }

        private readonly SchedulerSettings _schedulerSettings;
        private readonly ILogger<SchedulerComponent> _logger;
        private readonly IRobotsCache _robotsCache;
        private readonly IProducer<IngestRequest> _ingestRequestProducer;

        private readonly ConcurrentDictionary<Uri, bool> _visitedUris;
        private readonly ConcurrentDictionary<string, IEnumerable<string>> _visitedPathsLookup;
        private readonly ConcurrentDictionary<string, IEnumerable<Uri>> _queuedPathsLookup;
        private readonly SimplePriorityQueue<SchedulerQueueEntry, DateTimeOffset> _nextPathForHostQueue;

        private readonly IDomainParser _domainParser;

        public SchedulerComponent(SchedulerSettings schedulerSettings,
            IConsumer<SchedulerRequest> consumer,
            ILogger<SchedulerComponent> logger,
            IRobotsCache robotsCache,
            IProducer<IngestRequest> ingestRequestProducer)
            : base(consumer, logger, nameof(SchedulerComponent), schedulerSettings.MaxConcurrentRobotsRequests)
        {
            _schedulerSettings = schedulerSettings;
            _logger = logger;
            _robotsCache = robotsCache;
            _ingestRequestProducer = ingestRequestProducer;

            _visitedUris = new();
            _visitedPathsLookup = new();
            _queuedPathsLookup = new();
            _nextPathForHostQueue = new();

            _domainParser = new DomainParser(new WebTldRuleProvider());
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

                    AddNextUriToSchedulerQueue(entry.Domain, schedulerRequest);
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
            
            var domain = _domainParser.Parse(schedulerRequest.Uri).RegistrableDomain;
            var firstTimeVisit = !_visitedPathsLookup.Any(x => x.Key.EndsWith(domain, StringComparison.OrdinalIgnoreCase));

            _visitedPathsLookup.AddOrUpdate(schedulerRequest.Uri.Authority, schedulerRequest.Paths,
                (key, oldValue) =>
                {
                    visitedPathsForHost = oldValue;
                    var union = oldValue.Union(schedulerRequest.Paths);
                    return union;
                });

            var pathsToVisit = schedulerRequest.Paths.Except(visitedPathsForHost);

            if (pathsToVisit.Any() && (_schedulerSettings.RespectsRobotsTxt ?? false))
            {
                await _robotsCache.GetRobotsForHostAsync(schedulerRequest.Uri, robots => 
                {
                    pathsToVisit = pathsToVisit.Where(path =>
                    {
                        var fullUri = new Uri(schedulerRequest.Uri, path);
                        var allowed = robots.Allowed(fullUri);

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

            var urisToVisit = pathsToVisit.Select(path => new Uri(schedulerRequest.Uri, path)).ToArray();

            var alreadyQueued = false;
            _queuedPathsLookup.AddOrUpdate(domain, urisToVisit, (key, oldValue) => 
            {
                alreadyQueued = true;
                return oldValue.Union(urisToVisit);
            });

            if (!alreadyQueued) 
            {
                AddNextUriToSchedulerQueue(domain, schedulerRequest, firstTimeVisit);
            }                
        }

        private void AddNextUriToSchedulerQueue(string domain, SchedulerRequest schedulerRequest, bool first = false)
        {
            if (!_queuedPathsLookup.TryGetValue(domain, out var urisToVisit) || !urisToVisit.Any())
            {
                return;
            }

            var nextUriToVisit = urisToVisit.First();
            urisToVisit = urisToVisit.Skip(1);

            var queueEntry = new SchedulerQueueEntry(nextUriToVisit, domain, schedulerRequest);
                
            var notBefore = DateTimeOffset.Now;
            if (!first)
            {
                notBefore = notBefore.AddMilliseconds(_schedulerSettings.SameDomainCrawlDelayMillis);
            }

            _nextPathForHostQueue.Enqueue(queueEntry, notBefore);                
            
            if (urisToVisit.Any())
            {
                _queuedPathsLookup.AddOrUpdate(domain, urisToVisit, (key, oldValue) => urisToVisit);
            } 
            else
            {
                _queuedPathsLookup.TryRemove(domain, out _);
            }
        }
    }
}
