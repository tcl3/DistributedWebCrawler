using DistributedWebCrawler.Core.Tests.Customizations;

namespace DistributedWebCrawler.Core.Tests.Attributes
{
    internal class ByteCountingStreamAutoDataAttribute : MoqAutoDataAttribute
    {
        public ByteCountingStreamAutoDataAttribute(string streamContent = "") 
            : base(new ByteCountingStreamCustomization(streamContent))
        {

        }
    }
}
