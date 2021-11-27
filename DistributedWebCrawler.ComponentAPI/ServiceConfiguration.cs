using DistributedWebCrawler.Core.Interfaces;
using DistributedWebCrawler.Core.Seeding;
using NLog.Extensions.Logging;
using System.Text;
using DistributedWebCrawler.Core.Extensions.DependencyInjection;
using DistributedWebCrawler.Core;
using DistributedWebCrawler.Extensions.DependencyInjection;
using DistributedWebCrawler.Extensions.RabbitMQ;
using DistributedWebCrawler.Core.Robots;
using DistributedWebCrawler.Core.Model;
using DistributedWebCrawler.Extensions.Redis;
using DistributedWebCrawler.Extensions.DependencyInjection.Interfaces;

namespace DistributedWebCrawler.ComponentAPI
{
    internal static class ServiceConfiguration
    {
        public static IServiceProvider ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.ClearProviders();
                loggingBuilder.AddNLog();
            });

            var roles = GetRoles(configuration);
            var crawlerConfiguration = configuration.GetSection("CrawlerSettings");
            var crawlerAction = BuildCrawler(roles, crawlerConfiguration);
            services.AddCrawler(crawlerAction);

            services.AddRedisKeyValueStore(configuration);
            
            services.AddRabbitMQProducerConsumer(configuration);
            services.AddRabbitMQCrawlerManager(configuration);

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            return services.BuildServiceProvider();
        }

        private static Action<ICrawlerBuilder> BuildCrawler(IEnumerable<ComponentApiRole> roles, IConfiguration crawlerConfiguration)
        {
            return crawler =>
            {
                if (roles.Contains(ComponentApiRole.Scheduler))
                {
                    crawler.WithSeeder<SchedulerRequest>(seeder => seeder
                        .WithComponent<SchedulerQueueSeeder>()
                        .WithSettings(crawlerConfiguration.GetSection("SeederSettings")));

                    crawler.WithScheduler(scheduler => scheduler
                        .WithSettings(crawlerConfiguration.GetSection("SchedulerSettings")));
                }
                if (roles.Contains(ComponentApiRole.Ingester))
                {
                    crawler.WithIngester(ingester => ingester
                        .WithSettings(crawlerConfiguration.GetSection("IngesterSettings"))
                        .WithClient<CrawlerClient>(crawlerConfiguration.GetSection("CrawlerClientSettings")));
                }

                if (roles.Contains(ComponentApiRole.Parser))
                {
                    crawler.WithParser(parser => parser
                        .WithAngleSharpLinkParser()
                        .WithSettings(crawlerConfiguration.GetSection("ParserSettings")));
                }

                if (roles.Contains(ComponentApiRole.RobotsDownloader))
                {
                    crawler.WithRobotsDownloader(robots => robots
                        .WithSettings(crawlerConfiguration.GetSection("RobotsTxtSettings"))
                        .WithClient<RobotsClient>(crawlerConfiguration.GetSection("CrawlerClientSettings"), allowAutoRedirect: true));
                }
            };            
        }

        private enum ComponentApiRole
        {
            Scheduler,
            Ingester,
            Parser,
            RobotsDownloader
        }

        private static IEnumerable<ComponentApiRole> GetRoles(IConfiguration configuration)
        {
            var rolesString = configuration.GetValue<string>("COMPONENTAPI_ROLE");
            var rolesArray = rolesString?.Split(",", StringSplitOptions.RemoveEmptyEntries);
            if (rolesArray == null || !rolesArray.Any())
            {
                throw new InvalidOperationException("Variable COMPONENTAPI_ROLE not set. This variable should be a commma separated list of roles");
            }

            var roles = new List<ComponentApiRole>();
            foreach (var roleString in rolesArray)
            {
                var trimmedRole = roleString.Trim();
                if (!Enum.TryParse<ComponentApiRole>(trimmedRole, ignoreCase: true, out var role))
                {
                    throw new InvalidOperationException($"Invalid role found when parsing role configuration. '{trimmedRole}' is not a valid role name.");
                }
                roles.Add(role);
            }

            return roles;
        }
    }
}