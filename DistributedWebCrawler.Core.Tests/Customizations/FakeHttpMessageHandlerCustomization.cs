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
        private readonly string? _exceptionMessage;
        private readonly bool _isCancelled;

        public FakeHttpMessageHandlerCustomization(bool isCancelled = false)
        {
            _isCancelled = isCancelled;
        }

        public FakeHttpMessageHandlerCustomization(string? exceptionMessage)
        {
            _exceptionMessage = exceptionMessage ?? "Test Exception";
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
        }
    }
}
