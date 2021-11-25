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
using DistributedWebCrawler.Core.Queue;

namespace DistributedWebCrawler.Core.Components
{
    public class SchedulerComponent : AbstractTaskQueueComponent<SchedulerRequest>
    {
        private enum DomainStatus
        {   
            Inactive,
            Queued,
            Ingesting
        }

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

        private readonly IProducer<IngestRequest, IngestResult> _ingestRequestProducer;

        private readonly ConcurrentDictionary<Uri, bool> _visitedUris;
        private readonly ConcurrentDictionary<string, IEnumerable<string>> _visitedPathsLookup;
        private readonly ConcurrentDictionary<string, IEnumerable<Uri>> _queuedPathsLookup;
        private readonly ConcurrentDictionary<Guid, SchedulerQueueEntry> _activeQueueEntries;
        private readonly ConcurrentDictionary<string, DomainStatus> _activeDomains;
        private readonly InMemoryDateTimePriorityQueue<SchedulerQueueEntry> _nextPathForHostQueue;

        private readonly IDomainParser _domainParser;

        private readonly IEnumerable<DomainPattern> _domainsToInclude;
        private readonly IEnumerable<DomainPattern> _domainsToExclude;

        private const int IngestQueueMaxItems = 500;

        public SchedulerComponent(SchedulerSettings schedulerSettings,
            IConsumer<SchedulerRequest, bool> consumer,
            ILogger<SchedulerComponent> logger,
            IRobotsCache robotsCache,
            IProducer<IngestRequest, IngestResult> ingestRequestProducer)
            : base(consumer, logger, nameof(SchedulerComponent), schedulerSettings.MaxConcurrentRobotsRequests)
        {
            _schedulerSettings = schedulerSettings;
            _logger = logger;
            _robotsCache = robotsCache;
            _ingestRequestProducer = ingestRequestProducer;

            _visitedUris = new();
            _visitedPathsLookup = new();
            _queuedPathsLookup = new();
            _activeQueueEntries = new();
            _nextPathForHostQueue = new();
            _activeDomains = new();

            _domainParser = new DomainParser(new WebTldRuleProvider());

            _domainsToInclude = _schedulerSettings.IncludeDomains != null 
                ? _schedulerSettings.IncludeDomains.Select(str => new DomainPattern(str))
                : Enumerable.Empty<DomainPattern>();

            _domainsToExclude = _schedulerSettings.ExcludeDomains != null
                ? _schedulerSettings.ExcludeDomains.Select(str => new DomainPattern(str))
                : Enumerable.Empty<DomainPattern>();

            ingestRequestProducer.OnCompletedAsync += OnIngestCompletedAsync;
        }

        protected override Task ComponentStartAsync(CrawlerStartState startState)
        {
            var queueLoopTask = base.ComponentStartAsync(startState);
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
                try
                {
                    if (_ingestRequestProducer.Count > IngestQueueMaxItems)
                    {
                        // TODO: Replace this arbitrary delay with something better
                        await Task.Delay(100).ConfigureAwait(false);
                        continue;
                    }

                    var entry = await _nextPathForHostQueue.DequeueAsync().ConfigureAwait(false);

                    var schedulerRequest = entry.SchedulerRequest;

                    if (!_visitedUris.ContainsKey(entry.Uri))
                    {
                        _logger.LogDebug($"Enqueueing URI: {entry.Uri} for ingestion");

                        var ingestRequest = new IngestRequest(entry.Uri)
                        {
                            CurrentCrawlDepth = schedulerRequest.CurrentCrawlDepth,
                            MaxDepthReached = schedulerRequest.CurrentCrawlDepth >= _schedulerSettings.MaxCrawlDepth
                        };


                        if (!_activeQueueEntries.TryAdd(ingestRequest.Id, entry))
                        {
                            _logger.LogCritical($"Active queue item with ID: {ingestRequest.Id} already exists. This should never happen");
                            continue;
                        }
                        _activeDomains.AddOrUpdate(entry.Domain, DomainStatus.Queued, (key, oldvalue) => DomainStatus.Ingesting);
                        _ingestRequestProducer.Enqueue(ingestRequest);
                        _visitedUris.AddOrUpdate(entry.Uri, true, (key, oldValue) => oldValue);
                    }
                    else
                    {
                        await AddNextUriToSchedulerQueueAsync(entry.Domain, schedulerRequest, addCrawlDelay: true).ConfigureAwait(false);
                    }
                } 
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unhandled exception in Scheduler thread");
                }
            }
        }

        private async Task OnIngestCompletedAsync(object? sender, ItemCompletedEventArgs<IngestResult> eventArgs)
        {
            if (_activeQueueEntries.TryRemove(eventArgs.Id, out var entry))
            {
                if (_activeDomains.TryUpdate(entry.Domain, DomainStatus.Inactive, DomainStatus.Ingesting))
                {
                    if (!_activeDomains.TryRemove(entry.Domain, out var status) && status != DomainStatus.Inactive) 
                    {
                        _activeDomains.TryAdd(entry.Domain, status);
                    }
                }
                await AddNextUriToSchedulerQueueAsync(entry.Domain, entry.SchedulerRequest).ConfigureAwait(false);
            } 
            else
            {
                _logger.LogCritical($"No queue entry found for ingest request ID: {eventArgs.Id}");
            }
        }

        protected async override Task<bool> ProcessItemAsync(SchedulerRequest schedulerRequest)
        {
            if (schedulerRequest.CurrentCrawlDepth > _schedulerSettings.MaxCrawlDepth)
            {
                _logger.LogError($"Not processing {schedulerRequest.Uri}. Maximum crawl depth exceeded (curremt: {schedulerRequest.CurrentCrawlDepth}, max: {_schedulerSettings.MaxCrawlDepth})");
                return false;
            }

            var visitedPathsForHost = Enumerable.Empty<string>();
            
            var domain = _domainParser.IsValidDomain(schedulerRequest.Uri.Host) 
                ? _domainParser.Parse(schedulerRequest.Uri).RegistrableDomain
                : schedulerRequest.Uri.Host;

            var firstTimeVisit = !_visitedPathsLookup.Any(x => x.Key.EndsWith(domain, StringComparison.OrdinalIgnoreCase));

            _visitedPathsLookup.AddOrUpdate(schedulerRequest.Uri.Authority, schedulerRequest.Paths,
                (key, oldValue) =>
                {
                    visitedPathsForHost = oldValue;
                    var union = oldValue.Union(schedulerRequest.Paths);
                    return union;
                });

            var pathsToVisit = schedulerRequest.Paths.Except(visitedPathsForHost);

            if (pathsToVisit.Any() && _domainsToInclude.Any())
            {
                pathsToVisit = GetValidPaths(schedulerRequest.Uri, pathsToVisit, _domainsToInclude, PathCompareMode.Include);
            }

            if (pathsToVisit.Any() && _domainsToExclude.Any())
            {
                pathsToVisit = GetValidPaths(schedulerRequest.Uri, pathsToVisit, _domainsToExclude, PathCompareMode.Exclude);
            }

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
                return false;
            }

            schedulerRequest.Paths = pathsToVisit;

            var urisToVisit = pathsToVisit.Select(path => new Uri(schedulerRequest.Uri, path)).ToArray();

            _queuedPathsLookup.AddOrUpdate(domain, urisToVisit, (key, oldValue) => oldValue.Union(urisToVisit));

            if (!_activeDomains.TryGetValue(domain, out var status) || status == DomainStatus.Inactive)
            {
                await AddNextUriToSchedulerQueueAsync(domain, schedulerRequest, firstTimeVisit).ConfigureAwait(false);
            }

            return true;
        }

        private enum PathCompareMode
        {
            Include,
            Exclude,
        }

        private IEnumerable<string> GetValidPaths(Uri baseUri, IEnumerable<string> pathsToVisit, IEnumerable<DomainPattern> domainPatterns, PathCompareMode mode)
        {
            var validPaths = new List<string>();
            foreach (var path in pathsToVisit)
            {
                var fullUri = new Uri(baseUri, path);
                try
                {
                    var contains = domainPatterns.Any(pattern => pattern.Match(fullUri.Host));
                    if (mode == PathCompareMode.Include)
                    {
                        if (contains) validPaths.Add(path);
                        else _logger.LogDebug($"Excluding '{fullUri}'. Domain not in IncludeDomains list");
                    }
                    else if (mode == PathCompareMode.Exclude)
                    {
                        if (!contains) validPaths.Add(path);
                        else _logger.LogDebug($"Excluding '{fullUri}'. Domain is in ExcludeDomains list");
                    }
                } 
                catch (ParseException ex)
                {
                    _logger.LogError(ex, $"Unable to parse domain from {fullUri}");
                }
            }

            return validPaths;
        }

        private async Task AddNextUriToSchedulerQueueAsync(string domain, SchedulerRequest schedulerRequest, bool addCrawlDelay = false)
        {
            if (!_queuedPathsLookup.TryGetValue(domain, out var urisToVisit) || !urisToVisit.Any())
            {
                _queuedPathsLookup.TryRemove(domain, out _);
                return;
            }

            var nextUriToVisit = urisToVisit.First();
            urisToVisit = urisToVisit.Skip(1);

            var queueEntry = new SchedulerQueueEntry(nextUriToVisit, domain, schedulerRequest);
                
            var notBefore = DateTimeOffset.Now;
            if (!addCrawlDelay)
            {
                notBefore = notBefore.AddMilliseconds(_schedulerSettings.SameDomainCrawlDelayMillis);
            }

            await _nextPathForHostQueue.EnqueueAsync(queueEntry, notBefore).ConfigureAwait(false);

            _activeDomains.AddOrUpdate(queueEntry.Domain, DomainStatus.Queued, (key, oldvalue) => DomainStatus.Queued);
            
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
