using DistributedWebCrawler.Core.Extensions.DependencyInjection;
using DistributedWebCrawler.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace DistributedWebCrawler.Extensions.Redis
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRedisKeyValueStore(this IServiceCollection services, IConfiguration redisConfiguration, 
            IConfiguration redisConnectionPoolSettings)
        {
            services.AddRedisConnection(redisConfiguration, redisConnectionPoolSettings);
            services.AddSingleton<IKeyValueStore, RedisKeyValueStore>();
            return services;
        }

        private static IServiceCollection AddRedisConnection(this IServiceCollection services, IConfiguration redisConfiguration,
            IConfiguration redisConnectionPoolSettings)
        {
            services.AddSingleton<IConnectionMultiplexerPool, ConnectionMultiplexerPool>();
            services.AddSingleton<ConfigurationOptions>(s => GetConfigurationOptions(redisConfiguration));
            services.AddSettings<RedisConnectionPoolSettings>(redisConnectionPoolSettings);

            return services;
        }

        private static ConfigurationOptions GetConfigurationOptions(IConfiguration redisConfiguration)
        {
            var connectionString = redisConfiguration.GetValue<string>("REDIS_CONNECTIONSTRING");

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("Variable 'REDIS_CONNECTIONSTRING' not set");
            }
            
            return ConfigurationOptions.Parse(connectionString);
        }
    }
}