using System;

namespace DistributedWebCrawler.Core.Model
{
    public record ParseSuccess(Uri Uri, int NumberOfLinks);
}
