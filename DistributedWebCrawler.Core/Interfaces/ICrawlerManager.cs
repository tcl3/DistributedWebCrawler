using DistributedWebCrawler.Core.Enums;
using System.Threading.Tasks;

namespace DistributedWebCrawler.Core.Interfaces
{
    public interface ICrawlerManager
    {
        Task StartAsync(CrawlerRunningState startState = CrawlerRunningState.Running);
        Task WaitUntilCompletedAsync();
        Task WaitUntilCompletedAsync(ComponentFilter componentFilter);
        Task PauseAsync();
        Task PauseAsync(ComponentFilter componentFilter);
        Task ResumeAsync();
        Task ResumeAsync(ComponentFilter componentFilter);

        EventReceiverCollection Components { get; }
    }
}
