using DistributedWebCrawler.Core.Enums;
using DistributedWebCrawler.Core.Model;
using System;
using System.Threading.Tasks;

namespace DistributedWebCrawler.Core.Interfaces
{
    public interface ICrawlerManager
    {
        Task StartAsync(CrawlerStartState startState = CrawlerStartState.Running);
        Task WaitUntilCompletedAsync();
        Task PauseAsync();
        Task ResumeAsync();

        event AsyncEventHandler<PageCrawlSuccess> OnPageCrawlSuccess;
        
        event AsyncEventHandler<PageCrawlFailure> OnPageCrawlFailure;
    }
}
