using System.ComponentModel.DataAnnotations;

namespace DistributedWebCrawler.Extensions.DnsClient
{
    public class DnsResolverSettings
    {        
        public IEnumerable<string>? NameServers { get; init; }
        
        [Range(1, int.MaxValue)]
        public int? Retries { get; init; }
    }
}
