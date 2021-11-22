using DistributedWebCrawler.Core.Interfaces;
using DistributedWebCrawler.Core.Model;
using System.Threading.Tasks;

namespace DistributedWebCrawler.Core.Seeding
{
    public abstract class AbstractQueueSeeder<TData> : ISeeder
        where TData : RequestBase
    {
        private readonly IProducer<TData, bool> _producer;

        protected AbstractQueueSeeder(IProducer<TData, bool> producer)
        {
            _producer = producer;
        }

        protected abstract Task SeedQueueAsync(IProducer<TData, bool> producer);

        public async Task SeedAsync()
        {
            await SeedQueueAsync(_producer).ConfigureAwait(false);
        }
    }
}
