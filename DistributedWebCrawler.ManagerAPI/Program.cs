using Serilog;

namespace DistributedWebCrawler.ManagerAPI
{
    class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                var builder = WebApplication.CreateBuilder(args);

                var configuration = ServiceConfiguration.BuildConfiguration();

                var logger = new LoggerConfiguration()
                    .ReadFrom
                    .Configuration(configuration)
                    .CreateLogger();

                Log.Logger = logger;

                // Add services to the container.
                ServiceConfiguration.ConfigureServices(builder.Services, configuration, logger);

                var app = builder.Build();

                ServiceConfiguration.ConfigureMiddleware(app);
                
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