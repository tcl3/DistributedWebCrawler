using AutoFixture;
using DistributedWebCrawler.Core.Tests.Customizations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace DistributedWebCrawler.Core.Tests.Attributes
{
    internal class HttpClientAutoDataAttribute : MoqAutoDataAttribute
    {
        public HttpClientAutoDataAttribute(
            HttpStatusCode statusCode = HttpStatusCode.OK,
            string content = "",
            string? locationHeaderValue = null,
            string? contentTypeHeaderValue = null)
            : base(new ICustomization[]
            {
                new FakeHttpMessageHandlerCustomization(),
                new HttpClientCustomization(statusCode, content, locationHeaderValue, contentTypeHeaderValue)
            })
        {

        }

        public HttpClientAutoDataAttribute(
            string[] allowedUris,
            HttpStatusCode statusCode = HttpStatusCode.OK,
            string content = "",
            string? locationHeaderValue = null,
            string? contentTypeHeaderValue = null)
            : base(new ICustomization[]
            {
                new FakeHttpMessageHandlerCustomization(allowedUris.Select(uriString => new Uri(uriString))),
                new HttpClientCustomization(statusCode, content, locationHeaderValue, contentTypeHeaderValue)
            })
        {

        }
    }

    internal class ExceptionThrowingHttpClientAutoDataAttribute : MoqAutoDataAttribute
    {
        public ExceptionThrowingHttpClientAutoDataAttribute(string? exceptionMessage = null)
            : base(new ICustomization[]
            {
                new FakeHttpMessageHandlerCustomization(exceptionMessage),
                new HttpClientCustomization()
            })
        {

        }
    }

    internal class CancelledHttpClientAutoDataAttribute : MoqAutoDataAttribute
    {
        public CancelledHttpClientAutoDataAttribute() : base(new ICustomization[]
            {
                new FakeHttpMessageHandlerCustomization(isCancelled: true),
                new HttpClientCustomization()
            })
        {

        }
    }
}
