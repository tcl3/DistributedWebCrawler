using AutoFixture;
using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DistributedWebCrawler.Core.Tests.Customizations
{
    internal class HttpListenerCustomization : ICustomization
    {
        private readonly byte[] _responseBytes;

        public HttpListenerCustomization(string responseContent)
        {
            _responseBytes = Encoding.UTF8.GetBytes(responseContent);
        }

        public void Customize(IFixture fixture)
        {
            var listener = BindListenerOnFreePort();

            _ = HandleIncomingConnections(listener);
            
            fixture.Inject(listener);
        }

        // Modified from here: https://stackoverflow.com/a/46666370
        private static HttpListener BindListenerOnFreePort()
        {
            // IANA suggested range for dynamic or private ports
            const int MinPort = 49215;
            const int MaxPort = 65535;

            var portRange = Enumerable.Range(MinPort, MaxPort - MinPort + 1)
                .OrderBy(x => Random.Shared.Next());

            foreach (var port in portRange)
            {
                var listener = new HttpListener();
                listener.Prefixes.Add($"http://localhost:{port}/");
                try
                {
                    listener.Start();
                    return listener;
                }
                catch
                {
                    // nothing to do here -- the listener disposes itself when Start throws
                }
            }

            throw new Exception("Could not bind HttpListener to a free port");

        }

        private async Task HandleIncomingConnections(HttpListener listener)
        {
            listener.Start();
            
            while(true)
            {
                var context = await listener.GetContextAsync();
                var response = context.Response;

                var output = response.OutputStream;
                await output.WriteAsync(_responseBytes);

                output.Close();
            }
        }
    }
}
