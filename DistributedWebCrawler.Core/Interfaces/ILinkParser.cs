using DistributedWebCrawler.Core.LinkParser;
using DistributedWebCrawler.Core.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DistributedWebCrawler.Core.Interfaces
{
    public interface ILinkParser
    {
        Task<IEnumerable<Hyperlink>> ParseLinksAsync(IngestResult ingesterResult);
    }
}
