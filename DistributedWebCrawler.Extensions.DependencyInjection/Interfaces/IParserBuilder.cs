using DistributedWebCrawler.Core.Configuration;
using DistributedWebCrawler.Core.Interfaces;
using DistributedWebCrawler.Core.Model;

namespace DistributedWebCrawler.Extensions.DependencyInjection.Interfaces
{
    public interface IParserBuilder : IComponentBuilder<ParserSettings>
    {
        IParserBuilder WithLinkParser<TParser>() where TParser : class, ILinkParser;
    }
}
