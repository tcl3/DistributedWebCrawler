using DistributedWebCrawler.ManagerAPI.Hubs;

namespace DistributedWebCrawler.ManagerAPI
{
    class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var configuration = ServiceConfiguration.BuildConfiguration();
            
            // Add services to the container.
            ServiceConfiguration.ConfigureServices(builder.Services, configuration);

            var app = builder.Build();

            ServiceConfiguration.ConfigureMiddleware(app);

            app.Run();
        }
    }
}