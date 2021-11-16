using DistributedWebCrawler.Core.Enums;
using System.Threading.Tasks;

namespace DistributedWebCrawler.Core.Interfaces
{
    public interface ICrawlerComponent
    {
        Task StartAsync(CrawlerStartState startState = CrawlerStartState.Running);
        Task PauseAsync();
        Task ResumeAsync();
        Task WaitUntilCompletedAsync();
        CrawlerComponentStatus Status { get; }
        string Name { get; }
    }
}
