using DistributedWebCrawler.Core.Configuration;
using DistributedWebCrawler.Core.Enums;
using DistributedWebCrawler.Core.Extensions;
using DistributedWebCrawler.Core.Interfaces;
using DistributedWebCrawler.Core.LinkParser;
using DistributedWebCrawler.Core.Model;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;

namespace DistributedWebCrawler.Core.Components
{
    // FIXME: This should be backed by a thread pool. The current implementation doesn't allow for more than one document to be parsed at once.
    public class ParserCrawlerComponent : AbstractQueuedCrawlerComponent<ParseRequest>
    {
        private readonly IConsumer<ParseRequest> _parseRequestConsumer;

        private readonly IProducer<SchedulerRequest> _schedulerRequestProducer;
        private readonly ICrawlerComponentInterrogator _crawlerComponentInterrogator;
        private readonly ILinkParser _linkParser;
        private readonly ILogger<ParserCrawlerComponent> _logger;

        public ParserCrawlerComponent(ParserSettings parserSettings,
            IConsumer<ParseRequest> parseRequestConsumer,
            IProducer<SchedulerRequest> schedulerRequestProducer,
            ICrawlerComponentInterrogator crawlerComponentInterrogator,
            ILinkParser linkParser,
            ILogger<ParserCrawlerComponent> logger)
            : base(parseRequestConsumer, logger, nameof(ParserCrawlerComponent), parserSettings.MaxConcurrentThreads)
        {
            _parseRequestConsumer = parseRequestConsumer;
            _schedulerRequestProducer = schedulerRequestProducer;
            _crawlerComponentInterrogator = crawlerComponentInterrogator;
            _linkParser = linkParser;
            _logger = logger;
        }

        protected override async Task ProcessItemAsync(ParseRequest parseRequest)
        {
            var ingestResult = parseRequest.IngestResult;
            
            var links = await ParseLinksAsync(parseRequest.Uri, ingestResult).ConfigureAwait(false);

            var linksGroupedByHost = links.ToLookup(k => GetHostFromHref(k.Href, parseRequest.Uri), v => v.Href);

            foreach (var currentGroup in linksGroupedByHost.Where(x => x.Key != null))
            {
                var currentUri = currentGroup.Key;

                if (currentUri == null) continue;

                var paths = currentGroup.ToList();

                var authority = new Uri(currentUri.GetLeftPart(UriPartial.Authority), UriKind.Absolute);
                var schedulerRequest = new SchedulerRequest(authority)
                {
                    CurrentCrawlDepth = parseRequest.CurrentCrawlDepth + 1,
                    Paths = paths,
                };

                _logger.LogDebug($"Request sent to scheduler for host {currentUri}");

                _schedulerRequestProducer.Enqueue(schedulerRequest);                
            }
        }


        private Uri? GetHostFromHref(string href, Uri baseAddress)
        {
            if (!(Uri.TryCreate(href, UriKind.Absolute, out var absoluteUri) || Uri.TryCreate(baseAddress, href, out absoluteUri)))
            {
                _logger.LogInformation($"Error while parsing link: invalid URI {href}");
                return null;
            }

            if (absoluteUri.Scheme != Uri.UriSchemeHttps && absoluteUri.Scheme != Uri.UriSchemeHttp)
            {
                _logger.LogDebug($"Not following link with scheme type {absoluteUri.Scheme}");
                return null;
            }

            return new(absoluteUri.GetLeftPart(UriPartial.Authority));
        }

        private async Task<IEnumerable<Hyperlink>> ParseLinksAsync(Uri host, IngestResult ingestResult)
        {
            if (string.IsNullOrWhiteSpace(ingestResult.Content))
            {
                return Enumerable.Empty<Hyperlink>();
            }

            if (ingestResult.MediaType == MediaTypeNames.Text.Html)
            {
                var links = (await _linkParser.ParseLinksAsync(ingestResult).ConfigureAwait(false)).ToList();

                _logger.LogDebug($"{links.Count} links successfully parsed from domain {host} path {ingestResult.Path}");

                return links;
            }

            _logger.LogWarning($"Parsing of domain {host} path {ingestResult.Path} skipped due to unknown media type: {ingestResult.MediaType}");
            return Enumerable.Empty<Hyperlink>();
        }

        protected override CrawlerComponentStatus GetStatus()
        {
            // FIXME: Implement the correct status here to allow us to exit when done
            return _schedulerRequestProducer.IsEmpty() && _crawlerComponentInterrogator.AllOtherComponentsAre(Name, CrawlerComponentStatus.Completed)
                    ? CrawlerComponentStatus.Idle
                    : CrawlerComponentStatus.Busy;
        }
    }
}