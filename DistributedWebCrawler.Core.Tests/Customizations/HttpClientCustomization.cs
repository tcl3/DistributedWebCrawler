using AutoFixture;
using AutoFixture.Kernel;
using DistributedWebCrawler.Core.Tests.Fakes;
using System;
using System.Net.Http;

namespace DistributedWebCrawler.Core.Tests.Customizations
{

    public class HttpClientCustomization : ICustomization
    {
        public void Customize(IFixture fixture)
        {
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
}
