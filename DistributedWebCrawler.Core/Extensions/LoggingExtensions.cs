using DistributedWebCrawler.Core.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace DistributedWebCrawler.Core.Extensions
{
    public static class LoggingExtensions
    {
        public static IDisposable BeginComponentInfoScope(this ILogger logger, ComponentInfo componentInfo)
        {
            return logger.BeginScope(new Dictionary<string, object?>
            {
                ["componentName"] = componentInfo.ComponentName,
                ["componentId"] = componentInfo.ComponentId,
                ["nodeId"] = componentInfo.NodeId,
            });
        }

        public static IDisposable BeginRequestScope(this ILogger logger, RequestBase request)
        {
            return logger.BeginScope(new Dictionary<string, object?>
            {
                ["requestId"] = request.Id,
                ["requestUri"] = request.Uri,
                ["traceId"] = request.TraceId,
            });
        }

        public static IDisposable BeginIngestQueueScope(this ILogger logger, Uri ingestUri, SchedulerRequest schedulerRequest)
        {
            return logger.BeginScope(new Dictionary<string, object?>
            {
                ["ingestQueueUri"] = ingestUri,
                ["requestId"] = schedulerRequest.Id,
                ["requestUri"] = schedulerRequest.Uri,
                ["traceId"] = schedulerRequest.TraceId,
            });
        }
    }
}