using System.Net;

namespace DistributedWebCrawler.ComponentAPI
{
    class Program
    {
        static Program()
        {
            //ServicePointManager.DefaultConnectionLimit = 1000;
        }

        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var configuration = new ConfigurationBuilder()
                           .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                           .AddEnvironmentVariables()
                           .Build();

            ServiceConfiguration.ConfigureServices(builder.Services, configuration);

            builder.Services.AddHostedService<ComponentBackgroundService>();

            var app = builder.Build();

            app.Run();
        }
    }
}