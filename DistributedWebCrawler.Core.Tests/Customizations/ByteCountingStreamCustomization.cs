using AutoFixture;
using System.IO;
using System.Text;

namespace DistributedWebCrawler.Core.Tests.Customizations
{
    internal class ByteCountingStreamCustomization : ICustomization
    {
        private readonly string _streamContent;

        public ByteCountingStreamCustomization(string streamContent)
        {
            _streamContent = streamContent;
        }

        public void Customize(IFixture fixture)
        {
            fixture.Register<MemoryStream>(() =>
            {
                var ms = new MemoryStream();
                if (!string.IsNullOrEmpty(_streamContent))
                {
                    var bytesToWrite = Encoding.UTF8.GetBytes(_streamContent);
                    ms.Write(bytesToWrite);
                    ms.Position = 0;
                }

                return ms;
            });
        }
    }
}
