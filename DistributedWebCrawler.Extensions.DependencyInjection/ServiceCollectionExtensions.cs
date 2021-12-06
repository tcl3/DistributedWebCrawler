using DistributedWebCrawler.Core.Components;
using DistributedWebCrawler.Core.Interfaces;
using DistributedWebCrawler.Core.Model;
using DistributedWebCrawler.Core.Queue;
using DistributedWebCrawler.Core.Seeding;
using DistributedWebCrawler.Extensions.DependencyInjection;
using DistributedWebCrawler.Extensions.DependencyInjection.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DistributedWebCrawler.Core.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCrawler(this IServiceCollection services, Action<ICrawlerBuilder> crawlerAction)
        {
            var crawlerBuilder = new CrawlerBuilder(services);
            crawlerAction?.Invoke(crawlerBuilder);
            return services;
        }

        public static IServiceCollection AddInMemoryProducerConsumer(this IServiceCollection services)
        {
            services.AddSingleton(typeof(IProducerConsumer<>), typeof(InMemoryQueue<>));
            services.AddSingleton(typeof(IEventDispatcher<,>), typeof(InMemoryEventDispatcher<,>));
            services.AddSingleton(typeof(IEventReceiver<,>), typeof(InMemoryEventReceiver<,>));
            services.AddSingleton(typeof(InMemoryEventStore<,>));

            return services;
        }

        public static IServiceCollection AddInMemoryCrawlerManager(this IServiceCollection services, IEnumerable<Assembly>? componentAssemblies = null)
        {
            return services.AddCrawlerManager(typeof(InMemoryEventReceiver<,>), componentAssemblies);
        }

        public static IServiceCollection AddCrawlerManager(this IServiceCollection services, Type eventReceiverType, IEnumerable<Assembly>? componentAssemblies = null)
        {
            componentAssemblies ??= new[] { typeof(AbstractTaskQueueComponent<,,>).Assembly };

            services.AddSingleton<ISeeder, CompositeSeeder>();
            services.AddSingleton<ICrawlerManager, InMemoryCrawlerManager>();

            services.AddSingleton<IEventReceiverFactory>(serviceProvider =>
            {
                return new EventReceiverFactory(serviceProvider.GetRequiredService);
            });
            services.AddSingleton<EventReceiverCollection>();
            services.AddEventReceivers(componentAssemblies);

            services.AddDefaultSerializer();

            return services;
        }

        public static IServiceCollection AddEventReceivers(this IServiceCollection services, IEnumerable<Assembly> componentAssemblies)
        {
            var implementationType = services.Where(x => x.ServiceType == typeof(IEventReceiver<,>))
                .Select(x => x.ImplementationType)
                .OfType<Type>()
                .Single();

            var componentTypes = componentAssemblies
                .SelectMany(x => x.ExportedTypes)
                .Select(x => x.BaseType)
                .OfType<Type>()
                .Where(x =>
                {
                    return  x.IsAbstract
                            && !x.IsInterface
                            && x.IsGenericType
                            && x.GetGenericTypeDefinition() == typeof(AbstractTaskQueueComponent<,,>)
                            && x.GetGenericArguments().Length == 3;
                }
            );
            foreach (var componentType in componentTypes)
            {
                var genericArgs = componentType.GetGenericArguments();
                var successType = genericArgs[1];
                var failureType = genericArgs[2];

                var eventReceiverServiceType = typeof(IEventReceiver<,>).MakeGenericType(successType, failureType);
                var eventReceiverImplementationType = implementationType.MakeGenericType(successType, failureType);

                services.TryAddSingleton(eventReceiverServiceType, eventReceiverImplementationType);
                services.AddSingleton(typeof(IEventReceiver), s => s.GetRequiredService(eventReceiverServiceType));
            }

            return services;
        }

        public static IServiceCollection AddInMemoryKeyValueStore(this IServiceCollection services)
        {
            services.AddSingleton<IKeyValueStore, InMemoryKeyValueStore>();
            return services;
        }

        public static IServiceCollection AddSettings<TSettings>(this IServiceCollection services,
            IConfiguration configuration)  where TSettings : class
        {
            services.AddOptions<TSettings>()
                .Bind(configuration)
                .ValidateDataAnnotations();

            return services.AddSingleton<TSettings>(x => x.GetRequiredService<IOptions<TSettings>>().Value);
        }

        public static IServiceCollection AddSettings<TSettings>(this IServiceCollection services, 
            IConfiguration configuration, string sectionName)
            where TSettings : class
        {
            var configurationSection = configuration.GetSection(sectionName);
            return services.AddSettings<TSettings>(configurationSection);
        }

        public static IHttpClientBuilder AddHttpClientWithSettings<TClient, TSettings>(this IServiceCollection services, Action<HttpClient, TSettings> clientConfigurationAction)
            where TClient : class
            where TSettings : class
        {
            return services.AddHttpClient<TClient>((serviceProvider, client) =>
            {
                var settings = serviceProvider.GetRequiredService<TSettings>();
                clientConfigurationAction(client, settings);
            }).AddHttpMessageHandler(() => new FallbackEncodingHandler(Encoding.UTF8));
        }

        public static IServiceCollection AddDefaultSerializer(this IServiceCollection services)
        {
            services.TryAddSingleton<ISerializer, JsonSerializerAdaptor>();
            services.TryAddSingleton<JsonSerializerOptions>(_ => new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            });

            return services;
        }
    }
}
