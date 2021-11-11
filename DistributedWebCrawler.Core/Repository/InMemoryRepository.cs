using DistributedWebCrawler.Core.Entity;
using DistributedWebCrawler.Core.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace DistributedWebCrawler.Core.Repository
{

    public class InMemoryRepository<TKey, TEntity> : IRepository<TKey, TEntity> 
        where TEntity : BaseEntity<TKey>
        where TKey : struct
    {
        private readonly ConcurrentDictionary<TKey, TEntity> _objects;       

        public InMemoryRepository()
        {
            _objects = new();
        }

        public TEntity AddOrUpdate(TEntity obj)
        {
            if (EqualityComparer<TKey>.Default.Equals(obj.Key, default))
            {
                var keyToAdd = GetNextKey();
                //TBaseObject copy = (TBaseObject)obj.CopyOf();
                obj.Key = keyToAdd;
                if (!_objects.TryAdd(keyToAdd, obj))
                {
                    //throw new U
                }
                return obj; 
            } 
            else
            {
                return _objects.AddOrUpdate(obj.Key, obj, (key, existingValue) => existingValue.LastModified > obj.LastModified ? existingValue : obj);
            }
            
        }

        public IReadOnlyCollection<TEntity> GetAll()
        {
            return _objects.Values.ToList().AsReadOnly();
        }

        public bool TryGetById(TKey key, out TEntity? result)
        {
            if (EqualityComparer<TKey>.Default.Equals(key, default))
            {
                result = null;
                return false;
            }

            return _objects.TryGetValue(key, out result);
        }

        public TEntity GetOrAdd(TEntity obj)
        {
            return _objects.GetOrAdd(obj.Key, obj);
        }

        // TODO: Think of a better way to do this
        private static TKey GetNextKey()
        {
            if (typeof(TKey) == typeof(Guid))
            {
                return (TKey)(object)Guid.NewGuid();
            }

            throw new NotImplementedException($"GetNextKey() for type {typeof(TKey)} not implemented");
        }
    }
}
