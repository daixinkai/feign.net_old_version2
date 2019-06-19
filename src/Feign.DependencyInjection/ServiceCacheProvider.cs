using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using Feign.Discovery;
using Microsoft.Extensions.Caching.Distributed;

namespace Feign.Cache
{
    class ServiceCacheProvider : IServiceCacheProvider
    {
        public ServiceCacheProvider(IDistributedCache distributedCache)
        {
            _distributedCache = distributedCache;
        }

        IDistributedCache _distributedCache;

        public IList<IServiceInstance> Get(string name)
        {
            var instanceData = _distributedCache?.Get(name);
            if (instanceData != null && instanceData.Length > 0)
            {
                return DeserializeFromCache<List<SerializableIServiceInstance>>(instanceData).ToList<IServiceInstance>();
            }
            return null;
        }

        public async Task<IList<IServiceInstance>> GetAsync(string name)
        {
            var instanceData = await _distributedCache?.GetAsync(name);
            if (instanceData != null && instanceData.Length > 0)
            {
                return DeserializeFromCache<List<SerializableIServiceInstance>>(instanceData).ToList<IServiceInstance>();
            }
            return null;
        }

        public void Set(string name, IList<IServiceInstance> services, TimeSpan? slidingExpiration)
        {
            _distributedCache?.Set(name, SerializeForCache(MapToSerializable(services)), new DistributedCacheEntryOptions
            {
                SlidingExpiration = slidingExpiration
            });
        }

        public async Task SetAsync(string name, IList<IServiceInstance> services, TimeSpan? slidingExpiration)
        {
            await _distributedCache?.SetAsync(name, SerializeForCache(MapToSerializable(services)), new DistributedCacheEntryOptions
            {
                SlidingExpiration = slidingExpiration
            });
        }



        private static List<SerializableIServiceInstance> MapToSerializable(IList<IServiceInstance> instances)
        {
            var inst = instances.Select(i => new SerializableIServiceInstance(i));
            return inst.ToList();
        }

        private static byte[] SerializeForCache(object data)
        {
            using (var stream = new MemoryStream())
            {
                new BinaryFormatter().Serialize(stream, data);
                return stream.ToArray();
            }
        }

        private static T DeserializeFromCache<T>(byte[] data)
            where T : class
        {
            using (var stream = new MemoryStream(data))
            {
                return new BinaryFormatter().Deserialize(stream) as T;
            }
        }

    }
}
