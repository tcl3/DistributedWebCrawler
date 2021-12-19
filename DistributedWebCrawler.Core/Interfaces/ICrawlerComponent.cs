using DistributedWebCrawler.Core.Enums;
using System.Threading.Tasks;

namespace DistributedWebCrawler.Core.Interfaces
{
    public interface ICrawlerComponent
    {
        Task StartAsync(CrawlerRunningState startState = CrawlerRunningState.Running);
        Task PauseAsync();
        Task ResumeAsync();
        Task WaitUntilCompletedAsync();
        CrawlerComponentStatus Status { get; }
    }
}
