using AutoFixture;
using AutoFixture.Dsl;
using DistributedWebCrawler.Core.Tests.Fakes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace DistributedWebCrawler.Core.Tests.Customizations
{
    internal class FakeHttpMessageHandlerCustomization : ICustomization
    {
        private readonly HttpResponseEntry? _defaultResponseEntry;
        private readonly Dictionary<Uri, HttpResponseEntry?> _responseLookup;

        public FakeHttpMessageHandlerCustomization()
        {
            _responseLookup = new();
        }

        public FakeHttpMessageHandlerCustomization(IEnumerable<Uri> allowedUris)
        {
            _responseLookup = allowedUris.ToDictionary(k => k, v => (HttpResponseEntry?)null);
        }

        public FakeHttpMessageHandlerCustomization(bool isCancelled) : this()
        {
            _defaultResponseEntry = new HttpResponseEntry
            {
                IsCancelled = isCancelled,
            };
        }

        public FakeHttpMessageHandlerCustomization(string? exceptionMessage) : this()
        {
            _defaultResponseEntry = new HttpResponseEntry
            {
                Exception = new HttpRequestException(exceptionMessage ?? "Test Exception")
            };
        }

        public FakeHttpMessageHandlerCustomization(Dictionary<Uri, HttpResponseEntry?> responseLookup)
        {
            _responseLookup = responseLookup;
        }

        public void Customize(IFixture fixture)
        {
            fixture.Customize<FakeHttpMessageHandler>(composer => composer.FromFactory(() =>
            {
                var defaultResponseEntry = _defaultResponseEntry;
                if (defaultResponseEntry == null)
                {
                    defaultResponseEntry = new HttpResponseEntry
                    {
                        ResponseMessage = fixture.Create<HttpResponseMessage>(),
                    };
                }

                return new FakeHttpMessageHandler(_responseLookup, defaultResponseEntry);
            }));
        }
    }
}
