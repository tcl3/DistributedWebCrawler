using DistributedWebCrawler.Core.Enums;
using System.Threading.Tasks;

namespace DistributedWebCrawler.Core.Interfaces
{
    public interface ICrawlerComponent
    {
        Task StartAsync();
        Task PauseAsync();
        Task ResumeAsync();
        Task WaitUntilCompletedAsync();
        CrawlerComponentStatus Status { get; }
        string Name { get; }
    }
}
