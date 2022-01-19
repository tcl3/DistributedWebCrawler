using AutoFixture;
using AutoFixture.Xunit2;
using DistributedWebCrawler.Core.Tests.Customizations;

namespace DistributedWebCrawler.Core.Tests.Attributes
{
    internal class SerializerOptionsAutoDataAttribute : AutoDataAttribute
    {
        public SerializerOptionsAutoDataAttribute() 
            : base(() => new Fixture().Customize(new JsonSerializerOptionsCustomization()))
        {

        }
    }
}
