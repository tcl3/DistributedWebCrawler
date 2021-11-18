﻿using DistributedWebCrawler.Core.Configuration;
using DistributedWebCrawler.Core.Interfaces;
using DistributedWebCrawler.Core.Seeding;
using DistributedWebCrawler.Extensions.DependencyInjection.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace DistributedWebCrawler.Extensions.DependencyInjection
{
    public class SeederBuilder<TData> : ComponentBuilder<SeederSettings>, ISeederBuilder 
        where TData : class
    {
        public SeederBuilder(IServiceCollection services) : base(services)
        {
            services.AddSingleton<IProducer<TData>>(x => x.GetRequiredService<IProducerConsumer<TData>>());
        }

        ISeederBuilder ISeederBuilder.WithComponent<TComnponent>()
        {
            Services.AddSingleton<ISeederComponent, TComnponent>();
            return this;
        }
    }
}
