using DistributedWebCrawler.Core;
using DistributedWebCrawler.Core.Components;
using DistributedWebCrawler.Core.Extensions.DependencyInjection;
using DistributedWebCrawler.Core.Interfaces;
using DistributedWebCrawler.Core.Model;
using DistributedWebCrawler.Core.Robots;
using DistributedWebCrawler.Extensions.DependencyInjection.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DistributedWebCrawler.Extensions.DependencyInjection
{
    internal class CrawlerBuilder : ICrawlerBuilder
    {
        private readonly ISchedulerBuilder _schedulerBuilder;
        private readonly IIngesterBuilder _ingesterBuilder;
        private readonly IParserBuilder _parserBuilder;
        private readonly IRobotsDownloaderBuilder _robotsDownloaderBuilder;
        private readonly IServiceCollection _services;

        public CrawlerBuilder(IServiceCollection services)
        {
            _schedulerBuilder = new SchedulerBuilder(services);
            _ingesterBuilder = new IngesterBuilder(services);
            _parserBuilder = new ParserBuilder(services);
            _robotsDownloaderBuilder = new RobotsDownloaderBuilder(services);

            _services = services;

            RegisterCommonDefaultDependencies(services);
        }

        private static void RegisterCommonDefaultDependencies(IServiceCollection services)
        {
            services.TryAddSingleton(typeof(ComponentNameProvider<,>));
            services.TryAddSingleton<IContentStore, ContentStore>();
            services.TryAddSingleton<IRobotsCacheReader, RobotsCacheReader>();
            services.TryAddSingleton<RobotsCacheSettings>(serviceProvider =>
            {
                var robotsClient = serviceProvider.GetService<RobotsClient>();
                var settings = new RobotsCacheSettings 
                { 
                    UserAgent = robotsClient?.UserAgent 
                };

                return settings;
            });

            services.AddDefaultSerializer();
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

        public ICrawlerBuilder WithRobotsDownloader(Action<IRobotsDownloaderBuilder> robotsDownloaderAction)
        {
            _services.AddSingleton<ICrawlerComponent, RobotsDownloaderComponent>();
            _services.AddSingleton<IRobotsCacheWriter, RobotsCacheWriter>();
            robotsDownloaderAction?.Invoke(_robotsDownloaderBuilder);
            return this;
        }
    }
}
