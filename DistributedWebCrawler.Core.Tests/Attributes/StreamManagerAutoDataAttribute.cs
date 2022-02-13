using AutoFixture;
using DistributedWebCrawler.Core.Tests.Customizations;
using System;
using System.Net;
using System.Threading;

namespace DistributedWebCrawler.Core.Tests.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    internal class StreamManagerAutoDataAttribute : MoqAutoDataAttribute
    {
        public StreamManagerAutoDataAttribute(
            string responseContent = "Test Data",
            bool useCustomDns = false,
            string ipAddressToResolve = "127.0.0.1",
            bool customResolverThrowsException = false) 
            : base(GetCustomizations(
                responseContent, useCustomDns, ipAddressToResolve, customResolverThrowsException))
        {
        }

        private static ICustomization[] GetCustomizations(
            string responseContent, bool useCustomDns, string ipAddressToResolve,
            bool customResolverThrowsException)
        {
            Func<DnsEndPoint, CancellationToken, IPEndPoint>? dnsResolutionCallback = null;
            
            if (useCustomDns)
            {
                var ipAddress = IPAddress.Parse(ipAddressToResolve);
                dnsResolutionCallback = (dnsEndPoint, _) => new IPEndPoint(ipAddress, dnsEndPoint.Port);
            }

            return new ICustomization[]
            {
                new DnsResolverCustomization(dnsResolutionCallback, customResolverThrowsException),
                new HttpListenerCustomization(responseContent),
            };
        }
    }
}
