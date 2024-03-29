﻿using DistributedWebCrawler.Core.Enums;
using DistributedWebCrawler.Core.Models;
using System.Threading;
using System.Threading.Tasks;

namespace DistributedWebCrawler.Core.Interfaces
{
    public interface ICrawlerComponent
    {
        Task StartAsync(CrawlerRunningState startState = CrawlerRunningState.Running, CancellationToken cancellationToken = default);
        Task PauseAsync();
        Task ResumeAsync();
        Task WaitUntilCompletedAsync();
        CrawlerComponentStatus Status { get; }
        ComponentInfo ComponentInfo { get; }
    }
}
