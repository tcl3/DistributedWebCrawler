using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DistributedWebCrawler.Core
{
    public class FallbackEncodingHandler : DelegatingHandler
    {
        private readonly Encoding _fallbackEncoding;
        
        public FallbackEncodingHandler(Encoding fallbackEncoding)
        {
            _fallbackEncoding = fallbackEncoding;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)        
        {
            var message = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
            if (cancellationToken.IsCancellationRequested)
                return message;            

            // fix invalid encoding
            var charset = message.Content.Headers.ContentType?.CharSet;
            if (charset == null)
            {
                return message;
            }

            try
            {
                Encoding.GetEncoding(charset);
            }
            catch (ArgumentException)
            {
                using var responseStream = await message.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
                using var reader = new StreamReader(responseStream, _fallbackEncoding);
                message.Content = new StringContent(reader.ReadToEnd(), _fallbackEncoding);                
            }

            return message;
        }
    }
}