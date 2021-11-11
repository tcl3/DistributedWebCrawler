using DistributedWebCrawler.Core.Entity;
using System.Collections.Generic;

namespace DistributedWebCrawler.Core.Interfaces
{
    public interface IRepository<TKey, TBaseObject> 
        where TKey : struct
        where TBaseObject : BaseEntity<TKey>
    {
        TBaseObject GetOrAdd(TBaseObject domain);
        TBaseObject AddOrUpdate(TBaseObject domain);
        bool TryGetById(TKey key, out TBaseObject? result);
        IReadOnlyCollection<TBaseObject> GetAll();
    }
}
