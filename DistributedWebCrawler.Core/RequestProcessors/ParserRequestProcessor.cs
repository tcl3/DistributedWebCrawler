using DistributedWebCrawler.Core.Attributes;
using DistributedWebCrawler.Core.Extensions;
using DistributedWebCrawler.Core.Interfaces;
using DistributedWebCrawler.Core.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DistributedWebCrawler.Core.RequestProcessors
{
    [Component(name: "Parser", successType: typeof(ParseSuccess), failureType: typeof(ErrorCode<ParseFailure>))]
    public class ParserRequestProcessor : IRequestProcessor<ParseRequest>
    {
        private readonly IProducer<SchedulerRequest> _schedulerRequestProducer;
        private readonly ILinkParser _linkParser;
        private readonly IContentStore _contentStore;
        private readonly ILogger<ParserRequestProcessor> _logger;

        public ParserRequestProcessor(
            IProducer<SchedulerRequest> schedulerRequestProducer,
            ILinkParser linkParser,
            IContentStore contentStore,
            ILogger<ParserRequestProcessor> logger)
        {
            _schedulerRequestProducer = schedulerRequestProducer;
            _linkParser = linkParser;
            _contentStore = contentStore;
            _logger = logger;
        }

        public Task<QueuedItemResult> ProcessItemAsync(ParseRequest parseRequest, CancellationToken cancellationToken = default)
        {
            return Task.Run(async () => await ProcessItemInternalAsync(parseRequest, cancellationToken).ConfigureAwait(false), cancellationToken);
        }

        private async Task<QueuedItemResult> ProcessItemInternalAsync(ParseRequest parseRequest, CancellationToken cancellationToken)
        {
            var content = await _contentStore.GetContentAsync(parseRequest.ContentId, cancellationToken).ConfigureAwait(false);

            if (string.IsNullOrEmpty(content))
            {
                _logger.LogError("Item with ID: '{contentId}', not found in ContentStore", parseRequest.ContentId);
                return parseRequest.Failed(ParseFailure.NoItemInContentStore.AsErrorCode());
            }

            var links = (await _linkParser.ParseLinksAsync(content).ConfigureAwait(false)).ToList();

            if (!links.Any())
            {
                return parseRequest.Failed(ParseFailure.NoLinksFound.AsErrorCode());
            }

            _logger.LogDebug("{linkCount} links successfully parsed", links.Count);

            var linksGroupedByHost = links.ToLookup(k => GetHostFromHref(k.Href, parseRequest.Uri), v => v.Href);

            foreach (var currentGroup in linksGroupedByHost.Where(x => x.Key != null))
            {
                var currentUri = currentGroup.Key!;

                var paths = currentGroup.ToList();

                var authority = new Uri(currentUri.GetLeftPart(UriPartial.Authority), UriKind.Absolute);
                var schedulerRequest = new SchedulerRequest(authority)
                {
                    CurrentCrawlDepth = parseRequest.CurrentCrawlDepth + 1,
                    Paths = paths,
                    TraceId = Guid.NewGuid(),
                };

                _logger.LogDebug("Request sent to scheduler for host {uri}", currentUri);

                _schedulerRequestProducer.Enqueue(schedulerRequest);
            }

            return parseRequest.Success(new ParseSuccess(parseRequest.Uri) { NumberOfLinks = links.Count });
        }


        private Uri? GetHostFromHref(string href, Uri baseAddress)
        {
            // NB: URI.TryCreate behaviour is not consistent across platforms
            // A path such as "/path" with a leading forward slash is considered
            // an absolute path on Linux, but not on Windows or MacOS
            // See here: https://github.com/dotnet/runtime/issues/22718
            if (!((Uri.TryCreate(href, UriKind.Absolute, out var absoluteUri) && (!absoluteUri.IsFile)) || Uri.TryCreate(baseAddress, href, out absoluteUri)))
            {
                _logger.LogInformation("Error while parsing link: invalid URI {link}", href);
                return null;
            }

            if (absoluteUri.Scheme != Uri.UriSchemeHttps && absoluteUri.Scheme != Uri.UriSchemeHttp)
            {
                _logger.LogDebug("Not following link with scheme type {scheme}", absoluteUri.Scheme);
                return null;
            }

            return new(absoluteUri.GetLeftPart(UriPartial.Authority));
        }
    }
}