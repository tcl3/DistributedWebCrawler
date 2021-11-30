using System.Threading.Tasks;

namespace DistributedWebCrawler.Core
{
    public delegate Task AsyncEventHandler<TResult>(object? sender, TResult e);
}
