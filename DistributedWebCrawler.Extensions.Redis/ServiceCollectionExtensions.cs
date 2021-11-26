using DistributedWebCrawler.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using System.Collections.Concurrent;

namespace DistributedWebCrawler.Extensions.Redis
{
    public static class ServiceCollectionExtensions
    {
        private static ConcurrentDictionary<IConfiguration, IConnectionMultiplexer> _connectionMultiplexerLookup = new();
        public static IServiceCollection AddRedisKeyValueStore(this IServiceCollection services, IConfiguration redisConfiguration)
        {
            services.AddRedisCache(redisConfiguration);
            services.AddSingleton<IKeyValueStore, RedisKeyValueStore>();
            return services;
        }

        private static IServiceCollection AddRedisConnection(this IServiceCollection services, IConfiguration redisConfiguration)
        {
            services.AddSingleton<IConnectionMultiplexer>(s => GetConnectionMultiplexer(redisConfiguration));

            return services;
        }

        public static IServiceCollection AddRedisCache(this IServiceCollection services, IConfiguration redisConfiguration)
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.ConnectionMultiplexerFactory = () => Task.FromResult(GetConnectionMultiplexer(redisConfiguration));
            });

            return services;
        }

        private static IConnectionMultiplexer GetConnectionMultiplexer(IConfiguration redisConfiguration)
        {
            return _connectionMultiplexerLookup.GetOrAdd(redisConfiguration, CreateConnectionMultiplexer);
        }

        private static IConnectionMultiplexer CreateConnectionMultiplexer(IConfiguration redisConfiguration)
        {
            var connectionString = redisConfiguration.GetValue<string>("REDIS_CONNECTIONSTRING");

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("Variable 'REDIS_CONNECTIONSTRING' not set");
            }
            return ConnectionMultiplexer.Connect(connectionString);
        }
    }
}