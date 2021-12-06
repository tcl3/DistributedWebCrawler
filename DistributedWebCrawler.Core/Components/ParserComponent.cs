using DistributedWebCrawler.Core.Configuration;
using DistributedWebCrawler.Core.Interfaces;
using DistributedWebCrawler.Core.Model;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DistributedWebCrawler.Core.Components
{
    public class ParseSuccess
    {
        public Uri Uri { get; init; }
        public int NumberOfLinks { get; init; }

        public ParseSuccess(Uri uri, int numberOfLinks)
        {
            Uri = uri;
            NumberOfLinks = numberOfLinks;
        }
    }

    public enum ParseFailure
    {
        NoItemInContentStore,
        NoLinksFound
    }

    public class ParserComponent : AbstractTaskQueueComponent<ParseRequest, ParseSuccess, ParseFailure>
    {
        private readonly IConsumer<ParseRequest> _parseRequestConsumer;

        private readonly IProducer<SchedulerRequest> _schedulerRequestProducer;
        private readonly ILinkParser _linkParser;
        private readonly IContentStore _contentStore;
        private readonly ILogger<ParserComponent> _logger;

        public ParserComponent(ParserSettings parserSettings,
            IConsumer<ParseRequest> parseRequestConsumer,
            IEventDispatcher<ParseSuccess, ParseFailure> eventDispatcher,
            IProducer<SchedulerRequest> schedulerRequestProducer,
            ILinkParser linkParser,
            IContentStore contentStore,
            IKeyValueStore keyValueStore,
            ILogger<ParserComponent> logger)
            : base(parseRequestConsumer, eventDispatcher, keyValueStore, logger, nameof(ParserComponent), parserSettings)
        {
            _parseRequestConsumer = parseRequestConsumer;
            _schedulerRequestProducer = schedulerRequestProducer;
            _linkParser = linkParser;
            _contentStore = contentStore;
            _logger = logger;
        }

        protected override Task<QueuedItemResult> ProcessItemAsync(ParseRequest parseRequest, CancellationToken cancellationToken)
        {
            return Task.Run(async () => await ProcessItemInternalAsync(parseRequest, cancellationToken).ConfigureAwait(false), cancellationToken);
        }

        private async Task<QueuedItemResult> ProcessItemInternalAsync(ParseRequest parseRequest, CancellationToken cancellationToken)
        {
            var content = await _contentStore.GetContentAsync(parseRequest.ContentId, cancellationToken).ConfigureAwait(false);

            if (string.IsNullOrEmpty(content)) 
            {
                _logger.LogError($"Item with ID: '{parseRequest.ContentId}', not found in ContentStore");
                return Failed(parseRequest, ParseFailure.NoItemInContentStore);
            }

            var links = (await _linkParser.ParseLinksAsync(content).ConfigureAwait(false)).ToList();

            if (!links.Any())
            {
                return Failed(parseRequest, ParseFailure.NoLinksFound);
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

            return Success(parseRequest, new ParseSuccess(parseRequest.Uri, links.Count));
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
    }
}