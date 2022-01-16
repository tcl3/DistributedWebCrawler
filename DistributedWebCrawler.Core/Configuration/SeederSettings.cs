using System.Collections.Generic;

namespace DistributedWebCrawler.Core.Configuration
{
    public class SeederSettings
    {
        public IEnumerable<string>? UrisToCrawl { get; init; }
        public SeederSource Source { get; init; }
        public string FilePath { get; init; } = string.Empty;
    }

    public enum SeederSource
    {
        Unknown = 0,
        ReadFromFile,
        ReadFromConfig
    }
}
