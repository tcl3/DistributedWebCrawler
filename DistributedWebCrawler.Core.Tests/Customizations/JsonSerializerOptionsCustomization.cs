using AutoFixture;
using System.Text.Json;

namespace DistributedWebCrawler.Core.Tests.Customizations
{
    internal class JsonSerializerOptionsCustomization : ICustomization
    {
        private readonly JsonSerializerOptions? _options;

        public JsonSerializerOptionsCustomization(JsonSerializerOptions? options = null)
        {
            _options = options;
        }

        public void Customize(IFixture fixture)
        {
            fixture.Inject<JsonSerializerOptions?>(_options);
        }
    }
}
