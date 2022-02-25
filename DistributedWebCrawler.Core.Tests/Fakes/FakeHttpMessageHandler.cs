using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace DistributedWebCrawler.Core.Tests.Fakes
{
    internal class HttpResponseEntry
    {
        public HttpResponseMessage? ResponseMessage { get; set; }
        public HttpRequestException? Exception { get; set; }
        public bool IsCancelled { get; set; }
    }

    internal class FakeHttpMessageHandler : HttpMessageHandler
    {
        private readonly IDictionary<Uri, HttpResponseEntry?> _responseLookup;

        private readonly HttpResponseEntry? _defaultResponse;

        public FakeHttpMessageHandler(IDictionary<Uri, HttpResponseEntry?> responseLookup, HttpResponseEntry? defaultResponse)
        {
            _responseLookup = responseLookup;
            _defaultResponse = defaultResponse;
        }

        public FakeHttpMessageHandler(HttpResponseEntry defaultResponse)
        {
            _responseLookup = new Dictionary<Uri, HttpResponseEntry?>();
            _defaultResponse = defaultResponse;
        } 

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var responseTask = new TaskCompletionSource<HttpResponseMessage>();

            if (request.RequestUri == null || !_responseLookup.Any() 
                || (_responseLookup.TryGetValue(request.RequestUri, out var responseEntry) && responseEntry == null))
            {
                responseEntry = _defaultResponse ?? throw new InvalidOperationException($"No response set up for URI {request.RequestUri}");
            }

            if (responseEntry != null)
            {
                if (responseEntry.IsCancelled)
                {
                    responseTask.SetCanceled(cancellationToken);
                }
                else if (responseEntry.Exception != null)
                {
                    responseTask.SetException(responseEntry.Exception);
                }
                else if (responseEntry?.ResponseMessage != null)
                {
                    responseTask.SetResult(responseEntry.ResponseMessage);
                }
            }

            if (!responseTask.Task.IsCompleted)
            {
                throw new HttpRequestException($"Response not set for URI {request.RequestUri}");
            }

            return responseTask.Task;
        }
    }
}
