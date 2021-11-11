using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace DistributedWebCrawler.Core.Configuration
{
    public class SeederSettings
    {
        public IEnumerable<string>? UrisToCrawl { get; set; }
        public SeederSource Source { get; set; }
        public string FilePath { get; set; } = string.Empty;
    }

    public enum SeederSource
    {
        Unknown = 0,
        ReadFromFile,
        ReadFromConfig
    }
}
