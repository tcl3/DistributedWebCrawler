using AutoFixture;
using System.Text;

namespace DistributedWebCrawler.Core.Tests.Customizations
{
    internal class EncodingCustomization : ICustomization
    {
        private readonly Encoding _encoding;
        
        public EncodingCustomization(string encodingName)
        {
            _encoding = Encoding.GetEncoding(encodingName);
        }

        public void Customize(IFixture fixture)
        {
            fixture.Inject(_encoding);
        }
    }
}
