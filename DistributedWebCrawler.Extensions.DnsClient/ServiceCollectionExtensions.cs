using DistributedWebCrawler.Core.Extensions.DependencyInjection;
using DistributedWebCrawler.Core.Interfaces;
using DnsClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Net;

namespace DistributedWebCrawler.Extensions.DnsClient
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection UseCustomDns(this IServiceCollection services, IConfiguration dnsResolverConfiguration)
        {
            services.AddSettings<DnsResolverSettings>(dnsResolverConfiguration);
            services.AddSingleton<IDnsQuery>(serviceProvider =>
            {            
                var dnsResolverSettings = serviceProvider.GetRequiredService<DnsResolverSettings>();
                
                LookupClientOptions options;
                if (dnsResolverSettings.NameServers != null && dnsResolverSettings.NameServers.Any())
                {
                    var nameservers = ParseNameservers(dnsResolverSettings.NameServers);
                    options = new LookupClientOptions(nameservers.ToArray());
                }
                else
                {
                    options = new LookupClientOptions();
                }

                var nameServerList = new List<IPEndPoint>();

                if (dnsResolverSettings.Retries.HasValue)
                {
                    options.Retries = dnsResolverSettings.Retries.Value;
                }

                return new LookupClient(options);
            });

            services.AddSingleton<IDnsResolver, DnsClientWrapper>();

            return services;
        }

        private static IEnumerable<IPEndPoint> ParseNameservers(IEnumerable<string> nameServerStrings)
        {
            var nameServers = new List<IPEndPoint>();
            foreach (var nameServerString in nameServerStrings)
            {
                if (!IPEndPoint.TryParse(nameServerString, out var nameserver))
                {
                    throw new FormatException($"{nameServerString} is not a valid nameserver");
                }

                if (nameserver.Port == 0)
                {
                    // Default DNS port
                    nameserver.Port = 53;
                }

                nameServers.Add(nameserver);
            }

            return nameServers;
        }
    }
}
