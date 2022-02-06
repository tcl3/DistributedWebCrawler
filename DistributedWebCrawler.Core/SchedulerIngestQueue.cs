using DistributedWebCrawler.Core.Configuration;
using DistributedWebCrawler.Core.Extensions;
using DistributedWebCrawler.Core.Interfaces;
using DistributedWebCrawler.Core.Model;
using DistributedWebCrawler.Core.Queue;
using Microsoft.Extensions.Logging;
using Nager.PublicSuffix;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DistributedWebCrawler.Core
{
    public class SchedulerIngestQueue : ISchedulerIngestQueue
    {
        private readonly SchedulerSettings _schedulerSettings;
        private readonly IEventReceiver<IngestSuccess, IngestFailure> _ingestEventReceiver;
        private readonly ILogger<SchedulerIngestQueue> _logger;
        private readonly IProducer<IngestRequest> _ingestRequestProducer;
        private readonly IDomainParser _domainParser;
        private readonly ConcurrentDictionary<Uri, bool> _visitedUris;
        private readonly ConcurrentDictionary<string, IEnumerable<Uri>> _queuedPathsLookup;
        private readonly ConcurrentDictionary<Guid, SchedulerQueueEntry> _activeQueueEntries;
        private readonly ConcurrentDictionary<string, DomainStatus> _activeDomains;
        private readonly InMemoryDateTimePriorityQueue<SchedulerQueueEntry> _nextPathForHostQueue;

        private const int IngestQueueMaxItems = 500;

        private Task? _ingestQueueLoopTask;
        private object _schedulerLoopLock = new();

        private enum DomainStatus
        {
            Inactive,
            Queued,
            Ingesting
        }

        private record SchedulerQueueEntry(
            Uri Uri, 
            string Domain, 
            SchedulerRequest SchedulerRequest);

        public SchedulerIngestQueue(
            SchedulerSettings schedulerSettings,
            IEventReceiver<IngestSuccess, IngestFailure> ingestEventReceiver,
            ILogger<SchedulerIngestQueue> logger,
            IProducer<IngestRequest> ingestRequestProducer,
            IDomainParser domainParser)
        {
            _schedulerSettings = schedulerSettings;
            _ingestEventReceiver = ingestEventReceiver;
            _logger = logger;
            _ingestRequestProducer = ingestRequestProducer;
            _domainParser = domainParser;

            _visitedUris = new();

            _queuedPathsLookup = new();
            _activeQueueEntries = new();
            _nextPathForHostQueue = new();
            _activeDomains = new();
        }

        public Task AddFromSchedulerAsync(SchedulerRequest schedulerRequest, IEnumerable<Uri> urisToVisit,
            CancellationToken cancellationToken = default)
        {
            EnsureIngestQueueLoopStarted();

            var domain = _domainParser.IsValidDomain(schedulerRequest.Uri.Host)
                ? _domainParser.Parse(schedulerRequest.Uri).RegistrableDomain
                : schedulerRequest.Uri.Host;

            _queuedPathsLookup.AddOrUpdate(domain, urisToVisit,
                (key, oldValue) => oldValue.Union(urisToVisit));

            if (_activeDomains.TryGetValue(domain, out var status))
            {
                return Task.CompletedTask;
            }

            _queuedPathsLookup.AddOrUpdate(domain, urisToVisit,
                (key, oldValue) => oldValue.Union(urisToVisit));

            return AddNextUriToSchedulerQueueAsync(domain, schedulerRequest, addCrawlDelay: false, cancellationToken);
        }

        private async Task IngestQueueLoop(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    if (_ingestRequestProducer.Count > IngestQueueMaxItems)
                    {
                        // TODO: Replace this arbitrary delay with something better
                        await Task.Delay(100, cancellationToken).ConfigureAwait(false);
                        continue;
                    }

                    var entry = await _nextPathForHostQueue.DequeueAsync(cancellationToken).ConfigureAwait(false);

                    if (cancellationToken.IsCancellationRequested)
                    {
                        break;
                    }

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
                        _ingestRequestProducer.Enqueue(ingestRequest, _ingestEventReceiver, OnIngestCompletedAsync, OnIngestFailedAsync);
                        _visitedUris.AddOrUpdate(entry.Uri, true, (key, oldValue) => oldValue);
                    }
                    else
                    {
                        await AddNextUriToSchedulerQueueAsync(entry.Domain, schedulerRequest, 
                            addCrawlDelay: true, cancellationToken: cancellationToken).ConfigureAwait(false);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unhandled exception in Scheduler thread");
                }
            }
        }

        private async Task OnIngestCompletedAsync(object? sender, ItemCompletedEventArgs<IngestSuccess> eventArgs)
        {
            await UpdateActiveQueueEntries(eventArgs.Id).ConfigureAwait(false);
        }

        private async Task OnIngestFailedAsync(object? sender, ItemFailedEventArgs<IngestFailure> eventArgs)
        {
            await UpdateActiveQueueEntries(eventArgs.Id).ConfigureAwait(false);
        }

        private async Task UpdateActiveQueueEntries(Guid id)
        {
            if (_activeQueueEntries.TryRemove(id, out var entry))
            {
                if (_activeDomains.TryUpdate(entry.Domain, DomainStatus.Inactive, DomainStatus.Ingesting))
                {
                    if (!_activeDomains.TryRemove(entry.Domain, out var status) && status != DomainStatus.Inactive)
                    {
                        _activeDomains.TryAdd(entry.Domain, status);
                    }
                }

                await AddNextUriToSchedulerQueueAsync(entry.Domain, entry.SchedulerRequest, addCrawlDelay: true).ConfigureAwait(false);
            }
            else
            {
                _logger.LogCritical($"No queue entry found for ingest request ID: {id}");
            }
        }

        private void EnsureIngestQueueLoopStarted()
        {
            if (_ingestQueueLoopTask != null)
            {
                return;
            }

            lock (_schedulerLoopLock)
            {
                if (_ingestQueueLoopTask != null)
                {
                    return;
                }

                _ingestQueueLoopTask = Task.Run(() => IngestQueueLoop(CancellationToken.None));
            }
        }

        private async Task AddNextUriToSchedulerQueueAsync(string domain, SchedulerRequest schedulerRequest, 
            bool addCrawlDelay = true, CancellationToken cancellationToken = default)
        {
            if (!_queuedPathsLookup.TryGetValue(domain, out var urisToVisit) || !urisToVisit.Any())
            {
                _queuedPathsLookup.TryRemove(domain, out _);
                return;
            }

            var nextUriToVisit = urisToVisit.First();
            urisToVisit = urisToVisit.Skip(1);

            var queueEntry = new SchedulerQueueEntry(nextUriToVisit, domain, schedulerRequest);

            var notBefore = SystemClock.DateTimeOffsetNow();
            if (addCrawlDelay)
            {
                notBefore = notBefore.AddMilliseconds(_schedulerSettings.SameDomainCrawlDelayMillis);
            }

            await _nextPathForHostQueue.EnqueueAsync(queueEntry, notBefore, cancellationToken).ConfigureAwait(false);

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