﻿using System;
using System.Net;

namespace DistributedWebCrawler.Core.Models
{
    public class RedirectResult
    {
        public RedirectResult(Uri destinationUri, HttpStatusCode redirectStatusCode)
        {
            DestinationUri = destinationUri;
            RedirectStatusCode = redirectStatusCode;
        }

        public Uri DestinationUri { get; init; }
        public HttpStatusCode RedirectStatusCode { get; init; }
    }
}
