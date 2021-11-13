using DistributedWebCrawler.Core.Configuration;
using DistributedWebCrawler.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DistributedWebCrawler.Extensions.DependencyInjection.Interfaces
{
    public interface ISeederBuilder : IComponentBuilder<SeederSettings>
    {
        ISeederBuilder WithComponent<TComnponent>() where TComnponent : class, ISeederComponent;
    }
}
