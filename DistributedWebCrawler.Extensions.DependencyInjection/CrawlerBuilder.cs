using DistributedWebCrawler.Core.Components;
using DistributedWebCrawler.Core.Interfaces;
using DistributedWebCrawler.Core.Model;
using DistributedWebCrawler.Extensions.DependencyInjection.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace DistributedWebCrawler.Extensions.DependencyInjection
{
    internal class CrawlerBuilder : ICrawlerBuilder
    {
        private readonly ISchedulerBuilder _schedulerBuilder;
        private readonly IIngesterBuilder _ingesterBuilder;
        private readonly IParserBuilder _parserBuilder;
        private readonly IServiceCollection _services;

        public CrawlerBuilder(IServiceCollection services)
        {
            _schedulerBuilder = new SchedulerBuilder(services);
            _ingesterBuilder = new IngesterBuilder(services);
            _parserBuilder = new ParserBuilder(services);
            _services = services;
        }

        public ICrawlerBuilder WithSeeder<TRequest>(Action<ISeederBuilder> seederBuilderAction)
            where TRequest : RequestBase
        {
            var seederBuilder = new SeederBuilder<TRequest>(_services);
            seederBuilderAction?.Invoke(seederBuilder);
            return this;
        }

        public ICrawlerBuilder WithIngester(Action<IIngesterBuilder> ingesterBuilderAction)
        {
            _services.AddSingleton<ICrawlerComponent, IngesterComponent>();
            ingesterBuilderAction?.Invoke(_ingesterBuilder);
            return this;
        }

        public ICrawlerBuilder WithParser(Action<IParserBuilder> parserBuilderAction)
        {
            _services.AddSingleton<ICrawlerComponent, ParserComponent>();
            parserBuilderAction?.Invoke(_parserBuilder);
            return this;
        }

        public ICrawlerBuilder WithScheduler(Action<ISchedulerBuilder> schedulerBuilderAction)
        {
            _services.AddSingleton<ICrawlerComponent, SchedulerComponent>();
            schedulerBuilderAction?.Invoke(_schedulerBuilder);
            return this;
        }
    }
}
