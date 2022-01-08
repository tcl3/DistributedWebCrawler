using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace DistributedWebCrawler.Core.Configuration
{
    public class CrawlerClientSettings
    {
        [Required]
        [MinLength(1)]
        public string UserAgentString { get; init; } = string.Empty;
        public string? AcceptLanguage { get; init; }

        [Range(1, int.MaxValue, ErrorMessage = nameof(RequestTimeoutSeconds) + " must be a positive integer")]
        public int RequestTimeoutSeconds { get; init; }

        [Range(1, int.MaxValue, ErrorMessage = nameof(ConnectTimeoutSeconds) + " must be a positive integer")]
        public int ConnectTimeoutSeconds { get; init; }

        [Range(1, int.MaxValue, ErrorMessage = nameof(ResponseDrainTimeoutSeconds) + " must be a positive integer")]
        public int ResponseDrainTimeoutSeconds { get; init; }

        [Range(1, int.MaxValue, ErrorMessage = nameof(MaxConnectionsPerServer) + " must be a positive integer")]
        public int MaxConnectionsPerServer { get; init; }

        [DefaultValue(true)]
        public bool AllowRequestCompression { get; init; } = true;
    }
}