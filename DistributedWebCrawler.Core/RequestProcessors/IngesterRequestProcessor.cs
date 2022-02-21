using DistributedWebCrawler.Core.Attributes;
using DistributedWebCrawler.Core.Configuration;
using DistributedWebCrawler.Core.Enums;
using DistributedWebCrawler.Core.Extensions;
using DistributedWebCrawler.Core.Interfaces;
using DistributedWebCrawler.Core.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;

namespace DistributedWebCrawler.Core.RequestProcessors
{
    [Component(name: "Ingester", successType: typeof(IngestSuccess), failureType: typeof(IngestFailure))]
    public class IngesterRequestProcessor : IRequestProcessor<IngestRequest>
    {
        private readonly IngesterSettings _ingesterSettings;
        private readonly IProducer<ParseRequest> _parseRequestProducer;
        private readonly CrawlerClient _crawlerClient;
        private readonly IContentStore _contentStore;
        private readonly ILogger<IngesterRequestProcessor> _logger;

        private readonly Lazy<IEnumerable<MediaTypePattern>> _mediaTypesToInclude;
        private readonly Lazy<IEnumerable<MediaTypePattern>> _mediaTypesToExclude;

        private static readonly HashSet<string> ParseableMediaTypes = new()
        {
            MediaTypeNames.Text.Html,
            MediaTypeNames.Text.Plain
        };

        private class HandleRedirectResult
        {
            public HandleRedirectResult(Uri uri, HttpResponseMessage response)
            {
                Uri = uri;
                Response = response;
            }

            public Uri Uri { get; init; }
            public IngestFailureReason? FailureReason { get; init; }
            public IEnumerable<RedirectResult> Redirects { get; init; } = Enumerable.Empty<RedirectResult>();
            public HttpResponseMessage Response { get; init; }
        }

        public IngesterRequestProcessor(IngesterSettings ingesterSettings,
            IProducer<ParseRequest> parseRequestProducer,
            CrawlerClient crawlerClient,
            IContentStore contentStore,
            ILogger<IngesterRequestProcessor> logger)
        {
            _ingesterSettings = ingesterSettings;
            _parseRequestProducer = parseRequestProducer;
            _crawlerClient = crawlerClient;
            _contentStore = contentStore;
            _logger = logger;

            _mediaTypesToInclude = new Lazy<IEnumerable<MediaTypePattern>>(() => GetMediaTypes(ingesterSettings.IncludeMediaTypes));
            _mediaTypesToExclude = new Lazy<IEnumerable<MediaTypePattern>>(() => GetMediaTypes(ingesterSettings.ExcludeMediaTypes));
        }

        // TODO - move this to a custom configuration validator
        private static IEnumerable<MediaTypePattern> GetMediaTypes(IEnumerable<string>? patternStrings)
        {
            if (patternStrings == null || !patternStrings.Any())
            {
                return Enumerable.Empty<MediaTypePattern>();
            }

            var mediaTypePatterns = new List<MediaTypePattern>();
            foreach (var pattern in patternStrings)
            {
                if (MediaTypePattern.TryCreate(pattern, out var mediaTypePattern))
                {
                    mediaTypePatterns.Add(mediaTypePattern);
                }
                else
                {
                    throw new ArgumentException($"'{pattern}' is not a valid media type pattern");
                }
            }

            return mediaTypePatterns;
        }

        public async Task<QueuedItemResult> ProcessItemAsync(IngestRequest item, CancellationToken cancellationToken = default)
        {
            var requestStartTime = SystemClock.DateTimeOffsetNow();
            if (item.MaxDepthReached)
            {
                _logger.LogDebug("Not sending request to parser. Maximum depth reached");
                return item.Failed(IngestFailure.Create(item.Uri, requestStartTime, IngestFailureReason.MaxDepthReached));
            }

            try
            {
                var handleRedirectsResult = await GetAndHandleRedirectsAsync(item.Uri, cancellationToken).ConfigureAwait(false);

                var currentUri = handleRedirectsResult.Uri;

                if (handleRedirectsResult.FailureReason.HasValue)
                {
                    var ingestFailure = IngestFailure.Create(currentUri, requestStartTime, handleRedirectsResult.FailureReason.Value, redirects: handleRedirectsResult.Redirects);
                    return item.Failed(ingestFailure);
                }

                var response = handleRedirectsResult.Response;

                if (response.StatusCode.IsError())
                {
                    var statusCodeNumber = (int)response.StatusCode;
                    _logger.LogInformation("Ingest request failed with status code {statusCode}", statusCodeNumber);
                    var ingestFailure = IngestFailure.Create(currentUri, requestStartTime, IngestFailureReason.Http4xxError, httpStatusCode: response.StatusCode);
                    return item.Failed(ingestFailure);
                }

                if (response.Content.Headers.ContentLength > _ingesterSettings.MaxContentLengthBytes)
                {
                    _logger.LogInformation("Ingest request failed. ContentLength header too large. Header length ({headerLength} bytes) Max allowed ({maxAllowed} bytes)",
                        response.Content.Headers.ContentLength, _ingesterSettings.MaxContentLengthBytes);

                    return item.Failed(IngestFailure.Create(currentUri, requestStartTime, IngestFailureReason.ContentTooLarge));
                }

                var contentTypeHeader = response.Content.Headers.ContentType;
                if (contentTypeHeader?.MediaType != null && MediaTypePattern.TryCreate(contentTypeHeader.MediaType, out var contentType))
                {
                    if (_mediaTypesToInclude.Value.Any() && !_mediaTypesToInclude.Value.Any(x => x.Match(contentType)))
                    {
                        _logger.LogInformation("Ingest request failed. Content Type {contentType} not present in include list", contentTypeHeader.MediaType);
                        var ingestFailure = IngestFailure.Create(currentUri, requestStartTime, IngestFailureReason.MediaTypeNotPermitted, mediaType: contentTypeHeader.MediaType);
                        return item.Failed(ingestFailure);
                    }
                    else if (_mediaTypesToExclude.Value.Any() && _mediaTypesToExclude.Value.Any(x => x.Match(contentType)))
                    {
                        _logger.LogInformation("Ingest request failed. Content Type {contentType} present in exclude list", contentTypeHeader.MediaType);
                        var ingestFailure = IngestFailure.Create(currentUri, requestStartTime, IngestFailureReason.MediaTypeNotPermitted, mediaType: contentTypeHeader.MediaType);
                        return item.Failed(ingestFailure);
                    }
                }

                var urlContent = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

                var mediaType = response.Content.Headers.ContentType?.MediaType ?? string.Empty;

                var contentId = await _contentStore.SaveContentAsync(urlContent, cancellationToken).ConfigureAwait(false);

                var ingestResult = IngestSuccess.Success(currentUri,
                    requestStartTime,
                    contentId,
                    urlContent.Length,
                    mediaType,
                    response.StatusCode,
                    handleRedirectsResult.Redirects);

                if (ingestResult.ContentId.HasValue && ingestResult.MediaType != null)
                {
                    if (ParseableMediaTypes.Contains(ingestResult.MediaType))
                    {
                        var parseRequest = new ParseRequest(item.Uri, ingestResult.ContentId.Value, item.CurrentCrawlDepth)
                        {
                            TraceId = item.TraceId
                        };

                        _parseRequestProducer.Enqueue(parseRequest);
                    }
                    else
                    {
                        _logger.LogWarning("Not passing {uri} to parser. Non parseable content type", ingestResult.Uri);
                    }
                }

                _logger.LogDebug("Ingest request successful");

                return item.Success(ingestResult);
            }
            catch (HttpRequestException ex)
            {
                var exceptionMessage = ex.GetBaseException().Message;
                _logger.LogError("Ingest request failed. {exceptionMessage}", exceptionMessage);
                return item.Failed(IngestFailure.Create(item.Uri, requestStartTime, IngestFailureReason.NetworkConnectivityError, httpStatusCode: ex.StatusCode));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Error processing URL. Incorrect URI format");
                return item.Failed(IngestFailure.Create(item.Uri, requestStartTime, IngestFailureReason.UriFormatError));
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError("Timeout while processing ingest request {item.Uri}. {exceptionMessage}", ex.Message);
                return item.Failed(IngestFailure.Create(item.Uri, requestStartTime, IngestFailureReason.RequestTimeout));
            }
        }

        private async Task<HandleRedirectResult> GetAndHandleRedirectsAsync(Uri uri, CancellationToken cancellationToken)
        {
            var response = await _crawlerClient.GetAsync(uri, cancellationToken).ConfigureAwait(false);

            var currentRedirectDepth = 0;
            var redirects = new List<RedirectResult>();
            while (response.StatusCode.IsRedirect() && response.Headers.Location != null)
            {
                if (currentRedirectDepth++ >= _ingesterSettings.MaxRedirects)
                {
                    _logger.LogInformation("Ingest failed. Max redirect depth reached ({maxRedirects})", _ingesterSettings.MaxRedirects);
                    return new HandleRedirectResult(uri, response)
                    {
                        FailureReason = IngestFailureReason.MaxRedirectsReached,
                        Redirects = redirects
                    };
                }
                var redirectUri = response.Headers.Location;
                
                var statusCodeNumber = (int)response.StatusCode;
                _logger.LogDebug("Redirecting to {redirectUri} ({statusCode})", redirectUri, statusCodeNumber);

                var redirectUriAbsolute = PathToAbsoluteUri(uri, redirectUri.ToString());

                redirects.Add(new RedirectResult(redirectUriAbsolute, response.StatusCode));

                response = await _crawlerClient.GetAsync(redirectUriAbsolute, cancellationToken).ConfigureAwait(false);
                uri = redirectUriAbsolute;
            }

            return new HandleRedirectResult(uri, response) { Redirects = redirects };
        }

        private static Uri PathToAbsoluteUri(Uri baseAddress, string ingestPath)
        {
            return Uri.IsWellFormedUriString(ingestPath, UriKind.Absolute)
                ? new Uri(ingestPath)
                : new Uri(baseAddress, ingestPath);
        }
    }
}