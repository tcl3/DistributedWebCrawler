using AutoFixture;
using DistributedWebCrawler.Core.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DistributedWebCrawler.Core.Tests.Customizations
{
    public class SeederSettingsCustomization : ICustomization
    {
        private readonly IEnumerable<string>? _urisToCrawl;
        private readonly SeederSource _source;
        private readonly string _filePath;

        public SeederSettingsCustomization(
            IEnumerable<string>? urisToCrawl = null,
            SeederSource source = SeederSource.ReadFromConfig,
            string filePath = "")
        {
            _urisToCrawl = urisToCrawl;
            _source = source;
            _filePath = filePath;
        }

        public void Customize(IFixture fixture)
        {
            fixture.Customize<SeederSettings>(c => c
                .With(x => x.UrisToCrawl, _urisToCrawl ?? Enumerable.Empty<string>())
                .With(x => x.Source, _source)
                .With(x => x.FilePath, _filePath));
        }
    }
}
