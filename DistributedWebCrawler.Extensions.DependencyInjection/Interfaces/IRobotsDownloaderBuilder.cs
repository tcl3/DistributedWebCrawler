﻿using DistributedWebCrawler.Extensions.DependencyInjection.Configuration;

namespace DistributedWebCrawler.Extensions.DependencyInjection.Interfaces
{
    public interface IRobotsDownloaderBuilder : IComponentBuilder<AnnotatedRobotsTxtSettings>
    {
    }
}
