using Feign.Discovery;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Feign.Cache
{
    public interface IServiceCacheProvider
    {
        IList<IServiceInstance> Get(string name);
        void Set(string name, IList<IServiceInstance> services, TimeSpan? slidingExpiration);

        Task<IList<IServiceInstance>> GetAsync(string name);
        Task SetAsync(string name, IList<IServiceInstance> services, TimeSpan? slidingExpiration);

    }
}
