using DistributedWebCrawler.Core.Configuration;
using DistributedWebCrawler.Core.Interfaces;
using DistributedWebCrawler.Core.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DistributedWebCrawler.Core.Seeding
{
    public class SchedulerQueueSeeder : AbstractQueueSeeder<SchedulerRequest>, ISeederComponent
    {
        private readonly SeederSettings _seederSettings;

        public SchedulerQueueSeeder(IProducer<SchedulerRequest> ingestRequestProducer, SeederSettings seederSettings) 
            : base(ingestRequestProducer)
        {
            _seederSettings = seederSettings;

            if (_seederSettings.Source == SeederSource.Unknown)
            {
                throw new ArgumentException($"{nameof(seederSettings.Source)} should not be {nameof(SeederSource.Unknown)}");
            }

            if (_seederSettings.Source == SeederSource.ReadFromFile && string.IsNullOrEmpty(_seederSettings.FilePath))
            {
                throw new ArgumentException($"{nameof(seederSettings.FilePath)} should not be empty when {nameof(seederSettings.Source)} is set to {nameof(SeederSource.ReadFromFile)}");
            }

            if (_seederSettings.Source == SeederSource.ReadFromConfig && (!_seederSettings.UrisToCrawl?.Any() ?? false))
            {
                throw new ArgumentException($"{nameof(seederSettings.UrisToCrawl)} should not be empty when {nameof(seederSettings.Source)} is set to {nameof(SeederSource.ReadFromConfig)}");
            }
        }

        // This assumes we are starting from scratch for now
        protected async override Task SeedQueueAsync(IProducer<SchedulerRequest> producer)
        {
            var uris = await GetInitialUrisAsync().ConfigureAwait(false);

            foreach (var uri in uris)
            {
                var ingestRequest = new SchedulerRequest(new Uri(uri.GetLeftPart(UriPartial.Authority), UriKind.Absolute))
                { 
                    Paths = new[] { uri.PathAndQuery },
                    TraceId = Guid.NewGuid(),
                };

                producer.Enqueue(ingestRequest);
            }
        }

        private async Task<IEnumerable<Uri>> GetInitialUrisAsync()
        {
            string[] urlList;
            if (_seederSettings.Source == SeederSource.ReadFromFile)
            {
                urlList = await File.ReadAllLinesAsync(_seederSettings.FilePath).ConfigureAwait(false);
            } 
            else
            {
                urlList = _seederSettings.UrisToCrawl?.ToArray() ?? Array.Empty<string>();
            }
            
            var domainList = new List<Uri>();
            
            foreach (var urlString in urlList)
            {
                var uriBuilder = new UriBuilder(urlString);
                if (uriBuilder.Uri.IsAbsoluteUri)
                {
                    domainList.Add(uriBuilder.Uri);
                }                
            }

            return domainList;
        }
    }
}