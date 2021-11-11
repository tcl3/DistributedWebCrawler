using System.Net;

namespace DistributedWebCrawler.Core.Extensions
{
    internal static class HttpStatusCodeExtensions
    {
        public static bool IsError(this HttpStatusCode statusCode)
        {
            return statusCode >= HttpStatusCode.BadRequest;
        }

        public static bool IsRedirect(this HttpStatusCode statusCode)
        {
            return statusCode >= HttpStatusCode.MultipleChoices && statusCode < HttpStatusCode.BadRequest;
        }
    }
}
