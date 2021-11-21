using System;
using System.ComponentModel.DataAnnotations;

namespace DistributedWebCrawler.Core.Configuration
{
    public class CrawlerClientSettings
    {
        [Required]
        [MinLength(1)]
        public string UserAgentString { get; init; } = string.Empty;
        public string? AcceptLanguage { get; init; }
        
        [Range(1, int.MaxValue, ErrorMessage = nameof(TimeoutSeconds) + " must be a positive integer")]
        public int TimeoutSeconds { get; init; }
    }
}