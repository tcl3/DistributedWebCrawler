using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace DistributedWebCrawler.Core.Tests.Fakes
{
    public class FakeHttpMessageHandler : HttpMessageHandler
    {
        private readonly HttpResponseMessage _response;
        private HttpRequestException? _exceptionToThrow;

        public FakeHttpMessageHandler(HttpResponseMessage response)
        {
            _response = response;
        }

        public void SetException(HttpRequestException exceptionToThrow)
        {
            _exceptionToThrow = exceptionToThrow;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var responseTask = new TaskCompletionSource<HttpResponseMessage>();
            if (_exceptionToThrow != null)
            {
                responseTask.SetException(_exceptionToThrow);
            } 
            else
            {
                responseTask.SetResult(_response);
            }

            return responseTask.Task;
        }
    }
}
