using AutoFixture;
using DistributedWebCrawler.Core.Interfaces;
using Moq;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace DistributedWebCrawler.Core.Tests.Customizations
{
    internal class DnsResolverCustomization : ICustomization
    {
        private readonly Func<DnsEndPoint, CancellationToken, IPEndPoint>? _dnsResolutionFunction;
        private readonly bool _customResolverThrowsException;

        public DnsResolverCustomization(
            Func<DnsEndPoint, CancellationToken, IPEndPoint>? dnsResolutionFunction = null,
            bool customResolverThrowsException = false)
        {
            _dnsResolutionFunction = dnsResolutionFunction;
            _customResolverThrowsException = customResolverThrowsException;
        }

        public void Customize(IFixture fixture)
        {
            IDnsResolver? dnsResolver = null;
            if (_customResolverThrowsException)
            {
                var dnsResolverMock = new Mock<IDnsResolver>();
                dnsResolverMock
                    .Setup(x => x.ResolveAsync(It.IsAny<DnsEndPoint>(), It.IsAny<CancellationToken>()))
                    .Throws<SocketException>();

                dnsResolver = dnsResolverMock.Object;
            }
            else if (_dnsResolutionFunction != null)
            {
                var dnsResolverMock = new Mock<IDnsResolver>();
                dnsResolverMock
                    .Setup(x => x.ResolveAsync(It.IsAny<DnsEndPoint>(), It.IsAny<CancellationToken>()))
                    .Returns((DnsEndPoint endPoint, CancellationToken cancellationToken) =>
                    {
                        return ValueTask.FromResult(_dnsResolutionFunction.Invoke(endPoint, cancellationToken));
                    });
                
                dnsResolver = dnsResolverMock.Object;
            }

            fixture.Inject(dnsResolver);
        }
    }
}
