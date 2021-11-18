using DistributedWebCrawler.Core.Interfaces;
using System.Threading.Tasks;

namespace DistributedWebCrawler.Core.Seeding
{
    public abstract class AbstractQueueSeeder<TData> : ISeeder
        where TData : class
    {
        private readonly IProducer<TData> _producer;

        protected AbstractQueueSeeder(IProducer<TData> producer)
        {
            _producer = producer;
        }

        protected abstract Task SeedQueueAsync(IProducer<TData> producer);

        public async Task SeedAsync()
        {
            await SeedQueueAsync(_producer).ConfigureAwait(false);
        }
    }
}
