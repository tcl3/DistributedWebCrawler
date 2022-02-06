using DistributedWebCrawler.Core.Interfaces;
using DnsClient;
using System.Net;
using System.Net.Sockets;

namespace DistributedWebCrawler.Extensions.DnsClient
{
    public class DnsClientWrapper : IDnsResolver
    {
        private readonly IDnsQuery _dnsClient;

        public DnsClientWrapper(IDnsQuery resolver)
        {
            _dnsClient = resolver;
        }

        public async ValueTask<IPEndPoint> ResolveAsync(DnsEndPoint dnsEndPoint, CancellationToken cancellationToken = default)
        {
            var dnsResponse = await _dnsClient.QueryAsync(dnsEndPoint.Host, QueryType.A, cancellationToken: cancellationToken);
            var ipv4Addresses = dnsResponse.Answers
                .ARecords()
                .Select(x => x.Address);

            if (!ipv4Addresses.Any())
            {
                throw new SocketException((int)SocketError.HostNotFound);
            }

            return new IPEndPoint(ipv4Addresses.First(), dnsEndPoint.Port);
        }
    }
}
