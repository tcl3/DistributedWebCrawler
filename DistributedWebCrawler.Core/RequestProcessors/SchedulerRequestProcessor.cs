﻿using DistributedWebCrawler.Core.Configuration;
using DistributedWebCrawler.Core.Interfaces;
using DistributedWebCrawler.Core.Model;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nager.PublicSuffix;
using System.Threading;
using DistributedWebCrawler.Core.Attributes;
using DistributedWebCrawler.Core.Extensions;
using DistributedWebCrawler.Core.Models;

namespace DistributedWebCrawler.Core.RequestProcessors
{
    [Component(name: "Scheduler", successType: typeof(SchedulerSuccess), failureType: typeof(ErrorCode<SchedulerFailure>))]
    public class SchedulerRequestProcessor : IRequestProcessor<SchedulerRequest>
    {
        private record SchedulerQueueEntry(
            Uri Uri, 
            string Domain, 
            SchedulerRequest SchedulerRequest);

        private readonly SchedulerSettings _schedulerSettings;
        private readonly ILogger<SchedulerRequestProcessor> _logger;
        private readonly IRobotsCacheReader _robotsCacheReader;
        private readonly IProducer<RobotsRequest> _robotsRequestProducer;
        
        private readonly ConcurrentDictionary<string, IEnumerable<string>> _visitedPathsLookup;

        private readonly ISchedulerIngestQueue _schedulerIngestQueue;
        private readonly IEnumerable<DomainPattern> _domainsToInclude;
        private readonly IEnumerable<DomainPattern> _domainsToExclude;

        private enum PathCompareMode
        {
            Include,
            Exclude,
        }

        public SchedulerRequestProcessor(
            SchedulerSettings schedulerSettings,
            ILogger<SchedulerRequestProcessor> logger,
            IRobotsCacheReader robotsCacheReader,
            IProducer<RobotsRequest> robotsRequestProducer,
            ISchedulerIngestQueue schedulerIngestQueue)
        {
            _schedulerSettings = schedulerSettings;
            
            _logger = logger;
            _robotsCacheReader = robotsCacheReader;
            _robotsRequestProducer = robotsRequestProducer;
            _schedulerIngestQueue = schedulerIngestQueue;
            _visitedPathsLookup = new();

            _domainsToInclude = _schedulerSettings.IncludeDomains != null
                ? _schedulerSettings.IncludeDomains.Select(str => new DomainPattern(str))
                : Enumerable.Empty<DomainPattern>();

            _domainsToExclude = _schedulerSettings.ExcludeDomains != null
                ? _schedulerSettings.ExcludeDomains.Select(str => new DomainPattern(str))
                : Enumerable.Empty<DomainPattern>();
        }        

        public async Task<QueuedItemResult> ProcessItemAsync(SchedulerRequest schedulerRequest, CancellationToken cancellationToken = default)
        {
            // Scheduler queue start

            if (schedulerRequest.CurrentCrawlDepth > _schedulerSettings.MaxCrawlDepth)
            {
                _logger.LogError($"Not processing {schedulerRequest.Uri}. Maximum crawl depth exceeded (curremt: {schedulerRequest.CurrentCrawlDepth}, max: {_schedulerSettings.MaxCrawlDepth})");
                return schedulerRequest.Failed(SchedulerFailure.MaximumCrawlDepthReached.AsErrorCode());
            }

            var pathsToVisit = schedulerRequest.Paths;

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
                void IfRobotsExists(IRobots robots)
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
                }

                var exists = await _robotsCacheReader.GetRobotsTxtAsync(schedulerRequest.Uri, IfRobotsExists, cancellationToken).ConfigureAwait(false);
                if (!exists)
                {
                    var robotsRequest = new RobotsRequest(schedulerRequest.Uri, schedulerRequest.Id);
                    _robotsRequestProducer.Enqueue(robotsRequest);
                    return schedulerRequest.Waiting();
                }
            }
            
            var visitedPathsForHost = Enumerable.Empty<string>();
            _visitedPathsLookup.AddOrUpdate(schedulerRequest.Uri.Authority, schedulerRequest.Paths,
                (key, oldValue) =>
                {
                    visitedPathsForHost = oldValue;
                    var union = oldValue.Union(schedulerRequest.Paths);
                    return union;
                });

            pathsToVisit = pathsToVisit.Except(visitedPathsForHost);

            if (!pathsToVisit.Any())
            {
                _logger.LogDebug($"Not processing request for host: {schedulerRequest.Uri}. No unvisited paths");
                return schedulerRequest.Success(new SchedulerSuccess(schedulerRequest.Uri, Enumerable.Empty<string>()));
            }

            schedulerRequest.Paths = pathsToVisit;

            var urisToVisit = pathsToVisit.Select(path => new Uri(schedulerRequest.Uri, path)).ToArray();

            await _schedulerIngestQueue.AddFromSchedulerAsync(schedulerRequest, urisToVisit, cancellationToken).ConfigureAwait(false);

            return schedulerRequest.Success(new SchedulerSuccess(schedulerRequest.Uri, pathsToVisit));
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
    }
}