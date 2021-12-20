using NLog.Extensions.Logging;
using System.Text;
using DistributedWebCrawler.Extensions.RabbitMQ;
using DistributedWebCrawler.ManagerAPI.Hubs;
using Microsoft.AspNetCore.ResponseCompression;

namespace DistributedWebCrawler.ManagerAPI
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

            services.AddResponseCompression(options =>
            {
                options.EnableForHttps = true;
            });

            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "wwwroot";
            });

            services.AddRabbitMQCrawlerManager(configuration);

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
            return new ConfigurationBuilder()
                          .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                          .AddEnvironmentVariables()
                          .Build();
        }
    }
}