using AutoFixture;
using DistributedWebCrawler.Core.Tests.Customizations;
using System;

namespace DistributedWebCrawler.Core.Tests.Attributes
{
    internal class SchedulerIngestQueueAutoDataAttribute : MoqAutoDataAttribute
    {
        public SchedulerIngestQueueAutoDataAttribute(
            int expectedIngestQueueInvocations = 0,
            int sameDomainCrawlDelayMillis = 1,
            int timeoutMillis = 1000000,
            bool ingestCallbackSuccessful = true
            ) 
            : base(new ICustomization[]
            {
                new SchedulerSettingsCustomization(
                    paths: new[] {"/path1", "/path2"},
                    sameDomainCrawlDelayMillis: sameDomainCrawlDelayMillis),
                new SchedulerIngestQueueCustomization(
                    expectedIngestQueueInvocations: expectedIngestQueueInvocations,
                    timeout: TimeSpan.FromMilliseconds(timeoutMillis),
                    ingestCallbackSuccessful: ingestCallbackSuccessful)
            }, configureMembers: true)
        {
        }
    }
}
