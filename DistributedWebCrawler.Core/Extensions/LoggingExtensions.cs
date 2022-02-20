using DistributedWebCrawler.Core.Models;
using Microsoft.Extensions.Logging;
using System;

namespace DistributedWebCrawler.Core.Extensions
{
    public static class LoggingExtensions
    {
        public static IDisposable BeginComponentInfoScope(this ILogger logger, ComponentInfo componentInfo)
        {
            return logger.BeginScope("ComponentInfo (componentName: '{componentName}', Id: {componentId}, NodeId: {nodeId})", 
                componentInfo.ComponentName, componentInfo.ComponentId, componentInfo.NodeId);
        }

        public static IDisposable BeginRequestScope(this ILogger logger, RequestBase request)
        {
            return logger.BeginScope("Request (ID: {requestId}, URI: {requestUri})",
                request.Id, request.Uri);
        }
    }
}