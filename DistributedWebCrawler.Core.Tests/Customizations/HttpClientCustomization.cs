using AutoFixture;
using AutoFixture.Dsl;
using AutoFixture.Kernel;
using DistributedWebCrawler.Core.Tests.Fakes;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace DistributedWebCrawler.Core.Tests.Customizations
{
    internal class HttpClientCustomization : ICustomization
    {
        private readonly HttpStatusCode? _statusCode;
        private readonly string? _content;
        private readonly string? _locationHeaderValue;
        private readonly string? _contentTypeHeaderValue;
        private readonly string? _exceptionMessage;
        private readonly bool _isCancelled;

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

        public HttpClientCustomization(string? httpExceptionMessage)
        {
            _exceptionMessage = httpExceptionMessage ?? "Test exception";
        }

        protected HttpClientCustomization(bool isCancelled)
        {
            _isCancelled = isCancelled;
        }

        public void Customize(IFixture fixture)
        {
            fixture.Customize<FakeHttpMessageHandler>(composer => 
            {
                IPostprocessComposer<FakeHttpMessageHandler> result = composer;

                if (_exceptionMessage != null)
                {
                    var exception = new HttpRequestException(_exceptionMessage);
                    result = result.Do(x => x.SetException(exception));
                }
                
                if (_isCancelled)
                {
                    result = result.Do(x => x.SetCancelled(true));
                }

                return result;
            });

            fixture.Customize<HttpResponseMessage>(composer =>
            {
                IPostprocessComposer<HttpResponseMessage> result = composer;
                if (_statusCode != null)
                {
                    result = result.With(x => x.StatusCode, _statusCode.Value);
                }

                if (_content != null)
                {
                    result = result.With(x => x.Content, _contentTypeHeaderValue == null
                        ? new StringContent(_content)
                        : new StringContent(_content, Encoding.UTF8, _contentTypeHeaderValue)
                    );
                }

                if (_locationHeaderValue != null)
                {
                    result = result.Do(x => x.Headers.Location = new Uri(_locationHeaderValue));
                }

                return result;
            });

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

                var messageHandler = context.Create<FakeHttpMessageHandler>();

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

    internal class CancelledHttpClientCustomization : HttpClientCustomization 
    {
        public CancelledHttpClientCustomization() : base(isCancelled: true)
        {

        }
    }
}
