﻿using DistributedWebCrawler.Core;
using DistributedWebCrawler.Core.Components;
using DistributedWebCrawler.Core.Extensions.DependencyInjection;
using DistributedWebCrawler.Core.Interfaces;
using DistributedWebCrawler.Core.Model;
using DistributedWebCrawler.Core.Robots;
using DistributedWebCrawler.Extensions.DependencyInjection.Configuration;
using DistributedWebCrawler.Extensions.DependencyInjection.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Nager.PublicSuffix;
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
            services.TryAddSingleton<IComponentNameProvider, ComponentNameProvider>();
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

        public ICrawlerBuilder WithSeeder(Action<ISeederBuilder> seederBuilderAction)
        {
            var seederBuilder = new SeederBuilder(_services);
            seederBuilderAction?.Invoke(seederBuilder);
            return this;
        }

        public ICrawlerBuilder WithIngester(Action<IIngesterBuilder> ingesterBuilderAction)
        {
            _services.AddSingleton<ICrawlerComponent, TaskQueueComponent<IngestRequest, IngestSuccess, IngestFailure, AnnotatedIngesterSettings>>();
            _services.AddSingleton<IRequestProcessor<IngestRequest>, IngesterComponent>();
            ingesterBuilderAction?.Invoke(_ingesterBuilder);
            return this;
        }

        public ICrawlerBuilder WithParser(Action<IParserBuilder> parserBuilderAction)
        {
            _services.AddSingleton<ICrawlerComponent, TaskQueueComponent<ParseRequest, ParseSuccess, ErrorCode<ParseFailure>, AnnotatedParserSettings>>();
            _services.AddSingleton<IRequestProcessor<ParseRequest>, ParserComponent>();
            parserBuilderAction?.Invoke(_parserBuilder);
            return this;
        }

        public ICrawlerBuilder WithScheduler(Action<ISchedulerBuilder> schedulerBuilderAction)
        {
            _services.AddSingleton<ICrawlerComponent, TaskQueueComponent<SchedulerRequest, SchedulerSuccess, ErrorCode<SchedulerFailure>, AnnotatedSchedulerSettings>>();
            _services.AddSingleton<IRequestProcessor<SchedulerRequest>, SchedulerComponent>();

            _services.TryAddSingleton<IDomainParser>(_ => new DomainParser(new WebTldRuleProvider()));

            schedulerBuilderAction?.Invoke(_schedulerBuilder);
            return this;
        }

        public ICrawlerBuilder WithRobotsDownloader(Action<IRobotsDownloaderBuilder> robotsDownloaderAction)
        {
            _services.AddSingleton<ICrawlerComponent, TaskQueueComponent<RobotsRequest, RobotsDownloaderSuccess, ErrorCode<RobotsDownloaderFailure>, AnnotatedRobotsTxtSettings>>();
            _services.AddSingleton<IRequestProcessor<RobotsRequest>, RobotsDownloaderComponent>();

            _services.AddSingleton<IRobotsCacheWriter, RobotsCacheWriter>();
            robotsDownloaderAction?.Invoke(_robotsDownloaderBuilder);
            return this;
        }
    }
}
