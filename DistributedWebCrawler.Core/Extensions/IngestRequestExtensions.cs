using DistributedWebCrawler.Core.Model;
using System.Diagnostics.CodeAnalysis;

namespace DistributedWebCrawler.Core.Extensions
{
    public static class IngestRequestExtensions
    {
        public static bool TryGetPageCrawlSuccess(this IngestResult ingestResult, [NotNullWhen(returnValue: true)] out PageCrawlSuccess? result)
        {
            if (ingestResult.FailureReason == null)
            {
                result = new PageCrawlSuccess(ingestResult);
                return true;
            }

            result = null;
            return false;
        }

        public static bool TryGetPageCrawlFailure(this IngestResult ingestResult, [NotNullWhen(returnValue: true)] out PageCrawlFailure? result)
        {
            if (ingestResult.FailureReason != null)
            {
                result = new PageCrawlFailure(ingestResult);
                return true;
            }

            result = null;
            return false;
        }
    }
}