using DistributedWebCrawler.Core.Interfaces;
using DistributedWebCrawler.Core.Model;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DistributedWebCrawler.Core.LinkParser
{
    public abstract class LinkParserBase : ILinkParser
    {
        public async Task<IEnumerable<Hyperlink>> ParseLinksAsync(IngestResult ingesterResult)
        {
            if (string.IsNullOrWhiteSpace(ingesterResult.Content))
            {
                return Enumerable.Empty<Hyperlink>();
            }

            return await ParseLinksAsync(ingesterResult.Content).ConfigureAwait(false);
        }

        protected abstract Task<IEnumerable<Hyperlink>> ParseLinksAsync(string content);
    }
}
