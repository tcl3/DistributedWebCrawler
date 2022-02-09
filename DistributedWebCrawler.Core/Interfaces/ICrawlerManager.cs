using DistributedWebCrawler.Core.Enums;
using DistributedWebCrawler.Core.Models;
using System;
using System.Threading.Tasks;

namespace DistributedWebCrawler.Core.Interfaces
{
    public interface ICrawlerManager
    {
        Task StartAsync(CrawlerRunningState startState = CrawlerRunningState.Running);
        Task WaitUntilCompletedAsync();
        Task PauseAsync();
        Task ResumeAsync();

        EventReceiverCollection Components { get; }
    }
}
