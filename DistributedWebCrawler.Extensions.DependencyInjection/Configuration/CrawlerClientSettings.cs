using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace DistributedWebCrawler.Extensions.DependencyInjection.Configuration
{
    public class CrawlerClientSettings
    {
        [Required]
        [MinLength(1)]
        public virtual string UserAgentString { get; init; } = string.Empty;
        public virtual string? AcceptLanguage { get; init; }

        [Range(1, int.MaxValue, ErrorMessage = nameof(RequestTimeoutSeconds) + " must be a positive integer")]
        public virtual int RequestTimeoutSeconds { get; init; }

        [Range(1, int.MaxValue, ErrorMessage = nameof(ConnectTimeoutSeconds) + " must be a positive integer")]
        public virtual int ConnectTimeoutSeconds { get; init; }

        [Range(1, int.MaxValue, ErrorMessage = nameof(PooledConnectionLifetimeSeconds) + " must be a positive integer")]
        public virtual int PooledConnectionLifetimeSeconds { get; init; }
        
        [Range(1, int.MaxValue, ErrorMessage = nameof(PooledConnectionIdleTimeoutSeconds) + " must be a positive integer")]
        public virtual int PooledConnectionIdleTimeoutSeconds { get; init; }

        [Range(1, int.MaxValue, ErrorMessage = nameof(ResponseDrainTimeoutSeconds) + " must be a positive integer")]
        public virtual int ResponseDrainTimeoutSeconds { get; init; }

        [Range(1, int.MaxValue, ErrorMessage = nameof(MaxConnectionsPerServer) + " must be a positive integer")]
        public virtual int MaxConnectionsPerServer { get; init; }

        [DefaultValue(true)]
        public virtual bool AllowRequestCompression { get; init; } = true;
    }
}