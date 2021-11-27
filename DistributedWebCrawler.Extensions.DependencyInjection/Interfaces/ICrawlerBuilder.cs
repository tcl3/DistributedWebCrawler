using DistributedWebCrawler.Core.Model;

namespace DistributedWebCrawler.Extensions.DependencyInjection.Interfaces
{
    public interface ICrawlerBuilder
    {
        ICrawlerBuilder WithSeeder<TRequest>(Action<ISeederBuilder> seederBuilderAction) 
            where TRequest : RequestBase;
        ICrawlerBuilder WithScheduler(Action<ISchedulerBuilder> schedulerBuilderAction);
        ICrawlerBuilder WithIngester(Action<IIngesterBuilder> ingesterBuilderAction);
        ICrawlerBuilder WithParser(Action<IParserBuilder> parserBuilderAction);
        ICrawlerBuilder WithRobotsDownloader(Action<IRobotsDownloaderBuilder> robotsBuilderAction);
    }
}
