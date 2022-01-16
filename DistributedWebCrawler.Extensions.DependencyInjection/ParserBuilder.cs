using DistributedWebCrawler.Core.Interfaces;
using DistributedWebCrawler.Core.Model;
using DistributedWebCrawler.Extensions.DependencyInjection.Configuration;
using DistributedWebCrawler.Extensions.DependencyInjection.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace DistributedWebCrawler.Extensions.DependencyInjection
{
    internal class ParserBuilder : ComponentBuilder<ParseRequest, AnnotatedParserSettings>, IParserBuilder
    {
        public ParserBuilder(IServiceCollection services) : base(services)
        {
        }

        IParserBuilder IParserBuilder.WithLinkParser<TParser>()
        {
            Services.AddSingleton<ILinkParser, TParser>();
            return this;
        }
    }
}
