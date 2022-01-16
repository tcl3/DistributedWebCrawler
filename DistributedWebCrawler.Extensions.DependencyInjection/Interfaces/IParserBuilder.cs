using DistributedWebCrawler.Core.Interfaces;
using DistributedWebCrawler.Extensions.DependencyInjection.Configuration;

namespace DistributedWebCrawler.Extensions.DependencyInjection.Interfaces
{
    public interface IParserBuilder : IComponentBuilder<AnnotatedParserSettings>
    {
        IParserBuilder WithLinkParser<TParser>() where TParser : class, ILinkParser;
    }
}
