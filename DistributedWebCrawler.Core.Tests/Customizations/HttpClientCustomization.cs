using AutoFixture;
using AutoFixture.Dsl;
using AutoFixture.Kernel;
using DistributedWebCrawler.Core.Tests.Fakes;
using System;
using System.Net;
using System.Net.Http;
using System.Text;

namespace DistributedWebCrawler.Core.Tests.Customizations
{
    internal class HttpClientCustomization<THandler> : ICustomization
        where THandler : HttpMessageHandler
    {
        public virtual void Customize(IFixture fixture)
        {
            // This type relay is picked up in the case where THandler is a DelegatingHandler
            // In that case FakeHttpMessageHandler will be set as its InnerHandler
            fixture.Customizations.Add(new TypeRelay(
                typeof(HttpMessageHandler),
                typeof(FakeHttpMessageHandler)));

            fixture.Customizations.Add(new HttpClientSpecimenBuilder());
        }

        private class HttpClientSpecimenBuilder : ISpecimenBuilder
        {
            public HttpClientSpecimenBuilder(IRequestSpecification httpClientSpecification)
            {
                HttpClientSpecification = httpClientSpecification;
            }

            public HttpClientSpecimenBuilder() : this(new IsHttpClient())
            {
            }

            public IRequestSpecification HttpClientSpecification { get; }

            public object Create(object request, ISpecimenContext context)
            {
                if (request is null)
                {
                    throw new ArgumentNullException(nameof(request));
                }

                if (context is null)
                {
                    throw new ArgumentNullException(nameof(context));
                }

                if (!this.HttpClientSpecification.IsSatisfiedBy(request))
                {
                    return new NoSpecimen();
                }

                var messageHandler = context.Create<THandler>();

                return new HttpClient(messageHandler);
            }
        }

        private class IsHttpClient : IRequestSpecification
        {
            public bool IsSatisfiedBy(object request)
            {
                return request is Type type && type == typeof(HttpClient);
            }
        }
    }

    internal class HttpClientCustomization : HttpClientCustomization<FakeHttpMessageHandler>
    {
        private readonly HttpStatusCode? _statusCode;
        private readonly string? _content;
        private readonly string? _locationHeaderValue;
        private readonly string? _contentTypeHeaderValue;

        public HttpClientCustomization(
            HttpStatusCode statusCode = HttpStatusCode.OK,
            string content = "",
            string? locationHeaderValue = null,
            string? contentTypeHeaderValue = null)
        {
            _statusCode = statusCode;
            _content = content;
            _locationHeaderValue = locationHeaderValue;
            _contentTypeHeaderValue = contentTypeHeaderValue;
        }

        public override void Customize(IFixture fixture)
        {
            fixture.Customize<HttpResponseMessage>(composer =>
            {
                IPostprocessComposer<HttpResponseMessage> result = composer;
                if (_statusCode != null)
                {
                    result = result.With(x => x.StatusCode, _statusCode.Value);
                }

                if (_content != null)
                {
                    var content = new StringContent(_content);
                    
                    // Done this way so that potentially invalid content types can be used
                    if (_contentTypeHeaderValue != null)
                    {
                        content.Headers.Remove("Content-Type");
                        content.Headers.TryAddWithoutValidation("Content-Type", _contentTypeHeaderValue);
                    }

                    result = result.With(x => x.Content, content);
                }

                if (_locationHeaderValue != null)
                {
                    result = result.Do(x => x.Headers.Location = new Uri(_locationHeaderValue));
                }

                return result;
            });

            base.Customize(fixture);
        }
    }
}
