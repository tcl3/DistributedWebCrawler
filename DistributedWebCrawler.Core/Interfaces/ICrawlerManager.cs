using DistributedWebCrawler.Core.Enums;
using System.Threading.Tasks;

namespace DistributedWebCrawler.Core.Interfaces
{
    public interface ICrawlerManager
    {
        Task StartAsync();
        Task WaitUntilCompletedAsync();
        Task PauseAsync();
        Task ResumeAsync();
    }
}
