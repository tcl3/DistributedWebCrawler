using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DistributedWebCrawler.Core.Model
{
    public enum IngestFailureReason
    {
        None = 0,
        UnknownError,
        MaxDepthReached,
        Http4xxError,
        NetworkConnectivityError,
        UriFormatError,
        RequestTimeout,
        ContentTooLarge,
        MediaTypeNotPermitted,
        MaxRedirectsReached
    }
}
