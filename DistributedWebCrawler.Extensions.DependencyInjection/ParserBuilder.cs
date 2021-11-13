using DistributedWebCrawler.Core.Components;
using DistributedWebCrawler.Core.Configuration;
using DistributedWebCrawler.Core.Interfaces;
using DistributedWebCrawler.Core.Model;
using DistributedWebCrawler.Extensions.DependencyInjection.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace DistributedWebCrawler.Extensions.DependencyInjection
{
    public class ParserBuilder : ComponentBuilder<ParseRequest, SchedulerRequest, ParserSettings>, IParserBuilder
    {
        public ParserBuilder(IServiceCollection services) : base(services)
        {
            services.AddSingleton<ICrawlerComponent, ParserCrawlerComponent>();
        }

        IParserBuilder IParserBuilder.WithLinkParser<TParser>()
        {
            Services.AddSingleton<ILinkParser, TParser>();
            return this;
        }
    }
}
