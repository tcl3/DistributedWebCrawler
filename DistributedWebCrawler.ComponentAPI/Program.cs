using Serilog;

namespace DistributedWebCrawler.ComponentAPI
{
    class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                var builder = WebApplication.CreateBuilder(args);

                var configuration = new ConfigurationBuilder()
                               .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                               .AddEnvironmentVariables()
                               .Build();

                var logger = new LoggerConfiguration()
                        .ReadFrom
                        .Configuration(configuration)
                        .CreateLogger();

                Log.Logger = logger;

                ServiceConfiguration.ConfigureServices(builder.Services, configuration, logger);

                builder.Services.AddHostedService<ComponentBackgroundService>();

                var app = builder.Build();

                app.Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host terminated unexpectedly");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    }
}