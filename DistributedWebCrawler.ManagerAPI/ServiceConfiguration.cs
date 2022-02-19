using System.Text;
using DistributedWebCrawler.Extensions.RabbitMQ;
using DistributedWebCrawler.ManagerAPI.Hubs;
using DistributedWebCrawler.Core.Extensions.DependencyInjection;
using DistributedWebCrawler.Extensions.DnsClient;
using Serilog;

namespace DistributedWebCrawler.ManagerAPI
{
    internal static class ServiceConfiguration
    {
        public static IServiceProvider ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.ClearProviders();

                var logger = new LoggerConfiguration()
                    .ReadFrom
                    .Configuration(configuration)
                    .CreateLogger();

                loggingBuilder.AddSerilog(logger);
            });

            services.AddResponseCompression(options =>
            {
                options.EnableForHttps = true;
            });

            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "wwwroot";
            });

            if (configuration.GetValue<bool>("DevMode"))
            {
                services.UseCustomDns(configuration.GetSection("DnsResolverSettings"));
                services.AddInMemoryCrawlerManager();
                services.AddInMemoryCrawlerWithDefaultSettings(configuration);
            } 
            else
            {
                services.AddRabbitMQCrawlerManager(configuration);
            }            

            services.AddSignalR();
            services.AddSingleton<CrawlerHub>();
            services.AddSingleton<ComponentHubEventListener>();

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();
            
            services.AddHostedService<CrawlerBackgroundService>();

            return services.BuildServiceProvider();
        }

        public static WebApplication ConfigureMiddleware(WebApplication app)
        {
            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.MapHub<CrawlerHub>("/crawlerHub");

            app.UseResponseCompression();

            app.UseDefaultFiles();
            app.UseStaticFiles();

            app.UseAuthorization();

            app.MapControllers();

            app.UseSpa(spa =>
            {
                spa.Options.SourcePath = "app";
            });

            return app;
        }

        public static IConfiguration BuildConfiguration()
        {
            var builder = new ConfigurationBuilder();
            
            builder.AddJsonFile("appsettings.json", optional: false, reloadOnChange: false);
            
            var appSettings = builder.Build();

            if (appSettings.GetValue<bool>("DevMode"))
            {
                builder.AddJsonFile("crawlersettings.dev.json", optional: false, reloadOnChange: false);
            }

            builder.AddEnvironmentVariables();

            return builder.Build();
        }
    }
}