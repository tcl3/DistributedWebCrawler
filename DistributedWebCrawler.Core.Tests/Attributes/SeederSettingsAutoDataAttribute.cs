using DistributedWebCrawler.Core.Configuration;
using DistributedWebCrawler.Core.Tests.Customizations;
using System;

namespace DistributedWebCrawler.Core.Tests.Attributes
{
    internal class SeederSettingsAutoDataAttribute : MoqInlineAutoDataAttribute
    {
        public SeederSettingsAutoDataAttribute(
            string[]? urisToCrawl = null, 
            SeederSource source = SeederSource.ReadFromConfig,
            string filePath = "") 
            : base(new SeederSettingsCustomization(urisToCrawl, source, filePath), 
                  values: urisToCrawl == null 
                        ? Array.Empty<object>()
                        : new object[] { urisToCrawl })
        {

        }
    }
}
