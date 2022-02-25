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
        public static IServiceProvider ConfigureServices(IServiceCollection services,
            IConfiguration configuration, Serilog.ILogger logger)
        {
            services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.ClearProviders();

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

            var devMode = configuration.GetValue<bool?>("DEV_MODE") 
                ?? configuration.GetValue<bool?>("DevMode") 
                ?? false;

            if (devMode)
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
            app.Use(async (context, next) =>
            {
                if (context.Request.Path.Value == "/")
                {
                    context.Response.Redirect("/app");
                    return;
                }

                await next();
            });

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