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
using System.Threading;
using System.Threading.Tasks;

namespace DistributedWebCrawler.Core.Components
{
    public class ParserComponent : AbstractTaskQueueComponent<ParseRequest>
    {
        private readonly IConsumer<ParseRequest, bool> _parseRequestConsumer;

        private readonly IProducer<SchedulerRequest, bool> _schedulerRequestProducer;
        private readonly ILinkParser _linkParser;
        private readonly IContentStore _contentStore;
        private readonly ILogger<ParserComponent> _logger;

        public ParserComponent(ParserSettings parserSettings,
            IConsumer<ParseRequest, bool> parseRequestConsumer,
            IProducer<SchedulerRequest, bool> schedulerRequestProducer,
            ILinkParser linkParser,
            IContentStore contentStore,
            ILogger<ParserComponent> logger)
            : base(parseRequestConsumer, logger, nameof(ParserComponent), parserSettings)
        {
            _parseRequestConsumer = parseRequestConsumer;
            _schedulerRequestProducer = schedulerRequestProducer;
            _linkParser = linkParser;
            _contentStore = contentStore;
            _logger = logger;
        }

        protected override Task<bool> ProcessItemAsync(ParseRequest parseRequest, CancellationToken cancellationToken)
        {
            return Task.Run(async () => await ProcessItemInternalAsync(parseRequest, cancellationToken).ConfigureAwait(false), cancellationToken);
        }

        private async Task<bool> ProcessItemInternalAsync(ParseRequest parseRequest, CancellationToken cancellationToken)
        {
            var content = await _contentStore.GetContentAsync(parseRequest.ContentId, cancellationToken).ConfigureAwait(false);

            if (string.IsNullOrEmpty(content)) 
            {
                _logger.LogError($"Item with ID: '{parseRequest.ContentId}', not found in ContentStore");
                return false;
            }

            var links = (await _linkParser.ParseLinksAsync(content).ConfigureAwait(false)).ToList();

            if (!links.Any())
            {
                return false;
            }

            _logger.LogDebug($"{links.Count} links successfully parsed from URI {parseRequest.Uri}");

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

            return true;
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

        protected override CrawlerComponentStatus GetStatus()
        {
            // FIXME: Implement the correct status here to allow us to exit when done
            return CrawlerComponentStatus.Busy;
        }
    }
}