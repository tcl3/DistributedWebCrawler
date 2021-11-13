using DistributedWebCrawler.Core.Extensions.DependencyInjection;
using DistributedWebCrawler.Core.Queue;
using DistributedWebCrawler.Extensions.DependencyInjection.Interfaces;

namespace DistributedWebCrawler.Extensions.DependencyInjection
{
    public static class ComponentBuilderExtensions
    {
        public static IComponentBuilder<TRequest, TResult, TSettings> WithInMemoryProducerConsumer<TRequest, TResult, TSettings>(
            this IComponentBuilder<TRequest, TResult, TSettings> componentBuilder)
            where TRequest : class
            where TResult : class
            where TSettings : class
        {
            componentBuilder.Services.AddQueue<TRequest, InMemoryQueue<TRequest>>();
            return componentBuilder;
        }
    }
}
