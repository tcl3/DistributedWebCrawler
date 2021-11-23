using DistributedWebCrawler.Core.Configuration;
using DistributedWebCrawler.Core.Enums;
using DistributedWebCrawler.Core.Exceptions;
using DistributedWebCrawler.Core.Extensions;
using DistributedWebCrawler.Core.Interfaces;
using DistributedWebCrawler.Core.Model;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;

namespace DistributedWebCrawler.Core.Components
{
    public class IngesterComponent : AbstractTaskQueueComponent<IngestRequest>
    {
        private readonly IngesterSettings _ingesterSettings;
        private readonly IProducer<ParseRequest, bool> _parseRequestProducer;
        private readonly CrawlerClient _crawlerClient;
        private readonly IContentStore _contentStore;
        private readonly ILogger<IngesterComponent> _logger;

        private readonly IEnumerable<MediaTypePattern> _mediaTypesToInclude;
        private readonly IEnumerable<MediaTypePattern> _mediaTypesToExclude;

        private static readonly HashSet<string> ParseableMediaTypes = new() { MediaTypeNames.Text.Html, MediaTypeNames.Text.Plain };
        
        public IngesterComponent(IngesterSettings ingesterSettings,
            IConsumer<IngestRequest, bool> ingestRequestConsumer, 
            IProducer<ParseRequest, bool> parseRequestProducer,
            CrawlerClient crawlerClient,
            IContentStore contentStore,
            ILogger<IngesterComponent> logger) 
            : base(ingestRequestConsumer, logger, nameof(IngesterComponent), ingesterSettings.MaxDomainsToCrawl)
        {
            _ingesterSettings = ingesterSettings;
            _parseRequestProducer = parseRequestProducer;
            _crawlerClient = crawlerClient;
            _contentStore = contentStore;
            _logger = logger;

            _mediaTypesToInclude = GetMediaTypes(ingesterSettings.IncludeMediaTypes);
            _mediaTypesToExclude = GetMediaTypes(ingesterSettings.ExcludeMediaTypes);
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

        protected override CrawlerComponentStatus GetStatus()
        {
            // TODO: Implement the correct status here to allow us to exit when done
            return CrawlerComponentStatus.Busy;
        }

        protected async override Task<bool> ProcessItemAsync(IngestRequest item)
        {
            if (item.MaxDepthReached)
            {
                _logger.LogDebug($"Not sending request to parser for {item.Uri}");
                return false;
            }

            try
            {
                var ingestResult = await IngestCurrentPathAsync(item.Uri, allowRedirects: true).ConfigureAwait(false);

                if (!ingestResult.ContentId.HasValue || (!ParseableMediaTypes.Contains(ingestResult.MediaType)))
                {
                    _logger.LogWarning($"Not passing {ingestResult.Path} to parser. Non parseable content type");
                    return false;
                }

                var parseRequest = new ParseRequest(item.Uri, ingestResult)
                {
                    CurrentCrawlDepth = item.CurrentCrawlDepth
                };

                _parseRequestProducer.Enqueue(parseRequest);
            }
            catch (IngesterException ex)
            {
                _logger.LogInformation(ex.Message);
                return false;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, $"Error when getting for URI {item.Uri}. {ex.Message}");
                return false;
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, $"Error processing URL for URI {item.Uri}. {ex.Message}");
                return false;
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError($"Timeout while getting URL for URI {item.Uri}. {ex.Message}");
                return false;
            }

            return true;
        }

        private async Task<IngestResult> IngestCurrentPathAsync(Uri currentUri, bool allowRedirects)
        {   
            var response = await GetAndHandleRedirectsAsync(currentUri, allowRedirects).ConfigureAwait(false);

            if (response.StatusCode.IsError())
            {
                throw new IngesterException($"Failed to retrieve URI {currentUri} . Status code {(int)response.StatusCode}");
            }

            if (response.Content.Headers.ContentLength > _ingesterSettings.MaxContentLengthBytes)
            {
                throw new IngesterException($"Content length for {currentUri} ({response.Content.Headers.ContentLength} bytes) is longer than the maximum allowed ({_ingesterSettings.MaxContentLengthBytes} bytes)");
            }

            var contentTypeHeader = response.Content.Headers.ContentType;
            if (contentTypeHeader?.MediaType != null)
            {                
                if (contentTypeHeader.MediaType.Contains('*') || !MediaTypePattern.TryCreate(contentTypeHeader.MediaType, out var contentType))
                {
                    _logger.LogWarning($"Invalid Content-Type header for '{currentUri}' - {contentTypeHeader.MediaType}");
                }                
                else if (_mediaTypesToInclude.Any() && !_mediaTypesToInclude.Any(x => x.Match(contentType)))
                {
                    throw new IngesterException($"Content Type for '{currentUri}' ({contentTypeHeader.MediaType}) not present in include list");
                }
                else if (_mediaTypesToExclude.Any() && _mediaTypesToExclude.Any(x => x.Match(contentType)))
                {
                    throw new IngesterException($"Content Type for '{currentUri}' ({contentTypeHeader.MediaType}) present in exclude list");
                }
            }

            var urlContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            _logger.LogDebug($"Successfully retrieved content for URI {currentUri}");

            var mediaType = response.Content.Headers.ContentType?.MediaType ?? string.Empty;

            var contentId = await _contentStore.SaveContentAsync(urlContent).ConfigureAwait(false);

            return new IngestResult
            {
                Path = currentUri.ToString(),
                ContentId = contentId,
                MediaType = mediaType
            };            
        }

        private async Task<HttpResponseMessage> GetAndHandleRedirectsAsync(Uri uri, bool allowRedirects, int currentRedirectDepth = 0)
        {
            if (currentRedirectDepth >= _ingesterSettings.MaxRedirects)
            {
                throw new IngesterException($"Max redirect depth ({_ingesterSettings.MaxRedirects}) reached for URI: {uri}");
            }

            var response = await _crawlerClient.GetAsync(uri).ConfigureAwait(false);

            if (response.StatusCode.IsRedirect() && response.Headers.Location != null)
            {
                var redirectUri = response.Headers.Location;
                if (!allowRedirects)
                {
                    throw new IngesterException($"Not following redirect from {uri} to {redirectUri}. Redirects not allowed");
                }

                _logger.LogInformation($"URI {uri} is redirecting to {redirectUri} ({(int)response.StatusCode})");

                var redirectUriAbsolute = PathToAbsoluteUri(uri, redirectUri.ToString());
                return await GetAndHandleRedirectsAsync(redirectUriAbsolute, allowRedirects, currentRedirectDepth + 1).ConfigureAwait(false);
            }

            return response;
        }

        private static Uri PathToAbsoluteUri(Uri baseAddress, string ingestPath)
        {
            return Uri.IsWellFormedUriString(ingestPath, UriKind.Absolute)
                ? new Uri(ingestPath)
                : new Uri(baseAddress, ingestPath);
        } 
    }
}