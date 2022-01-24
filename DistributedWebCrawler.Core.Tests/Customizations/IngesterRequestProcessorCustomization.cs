using AutoFixture;
using DistributedWebCrawler.Core.Configuration;
using DistributedWebCrawler.Core.Model;

namespace DistributedWebCrawler.Core.Tests.Customizations
{
    internal class IngesterRequestProcessorCustomization : ICustomization
    {
        private readonly bool _maxDepthReached;
        private readonly string[]? _includeMediaTypes;
        private readonly string[]? _excludeMediaTypes;
        private readonly int? _maxContentLengthBytes;
        private readonly int _maxConcurrentItems;
        private readonly int _maxRedirects;

        public IngesterRequestProcessorCustomization(
            bool maxDepthReached = false,
            string[]? includeMediaTypes = null,
            string[]? excludeMediaTypes = null,
            int? maxContentLengthBytes = null,
            int maxConcurrentItems = 1,
            int maxRedirects = 1
        )
        {
            _maxDepthReached = maxDepthReached;
            _includeMediaTypes = includeMediaTypes;
            _excludeMediaTypes = excludeMediaTypes;
            _maxContentLengthBytes = maxContentLengthBytes;
            _maxConcurrentItems = maxConcurrentItems;
            _maxRedirects = maxRedirects;
        }

        public void Customize(IFixture fixture)
        {
            fixture.Customize<IngestRequest>(c => c
                .With(x => x.MaxDepthReached, _maxDepthReached)                
            );

            fixture.Customize<IngesterSettings>(c => c
                .With(x => x.IncludeMediaTypes, _includeMediaTypes)
                .With(x => x.ExcludeMediaTypes, _excludeMediaTypes)
                .With(x => x.MaxContentLengthBytes, _maxContentLengthBytes)
                .With(x => x.MaxConcurrentItems, _maxConcurrentItems)
                .With(x => x.MaxRedirects, _maxRedirects)
            );
        }
    }
}
