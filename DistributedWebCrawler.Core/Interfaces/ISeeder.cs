using System.Threading.Tasks;

namespace DistributedWebCrawler.Core.Interfaces
{
    public interface ISeeder 
    {
        Task SeedAsync();
    }

    public interface ISeederComponent : ISeeder
    {

    }
}
