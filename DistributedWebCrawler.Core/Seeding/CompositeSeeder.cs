using DistributedWebCrawler.Core.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DistributedWebCrawler.Core.Seeding
{
    public class CompositeSeeder : ISeeder
    {
        private readonly IEnumerable<ISeeder> _seederComponents;

        public CompositeSeeder(IEnumerable<ISeederComponent> seederComponents)
        {
            _seederComponents = seederComponents;
        }

        public async Task SeedAsync()
        {
            foreach (var seeder in _seederComponents)
            {
                await seeder.SeedAsync().ConfigureAwait(false);
            }
        }
    }
}
