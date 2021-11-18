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

namespace DistributedWebCrawler.ComponentAPI
{
    internal static class ServiceConfiguration
    {
        public static IServiceProvider ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            var roles = GetRoles(configuration);

            var crawlerConfiguration = configuration.GetSection("CrawlerSettings");

            services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.ClearProviders();
                loggingBuilder.AddNLog();
            });

            services.AddSingleton<ISeeder, CompositeSeeder>();
            if (roles.Contains(ComponentApiRole.Scheduler))
            {
                services.AddSeeder<SchedulerRequest>()
                    .WithComponent<SchedulerQueueSeeder>()
                    .WithSettings(crawlerConfiguration.GetSection("SeederSettings"));

                services.AddScheduler()
                    .WithRobotsCache<InMemoryRobotsCache>(crawlerConfiguration.GetSection("RobotsTxtSettings"))
                    .WithSettings(crawlerConfiguration.GetSection("SchedulerSettings"))
                    .WithClient<RobotsClient>(crawlerConfiguration.GetSection("CrawlerClientSettings"));
            }
            if (roles.Contains(ComponentApiRole.Ingester))
            {
                services.AddIngester()
                    .WithSettings(crawlerConfiguration.GetSection("IngesterSettings"))
                    .WithClient<CrawlerClient>(crawlerConfiguration.GetSection("CrawlerClientSettings"));
            }

            if (roles.Contains(ComponentApiRole.Parser))
            {
                services.AddParser()
                    .WithAngleSharpLinkParser()
                    .WithSettings(crawlerConfiguration.GetSection("ParserSettings"));
            }

            services.AddSingleton<ICrawlerManager, InMemoryCrawlerManager>();

            services.AddRabbitMQProducerConsumer(configuration);
            services.AddRabbitMQManager(configuration);

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            return services.BuildServiceProvider();
        }

        private enum ComponentApiRole
        {
            Scheduler,
            Ingester,
            Parser,
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