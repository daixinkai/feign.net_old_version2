using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Feign.Discovery;

namespace Feign.Cache
{
    public class DefaultServiceCacheProvider : IServiceCacheProvider
    {
        public IList<IServiceInstance> Get(string name)
        {
            return null;
        }

        public Task<IList<IServiceInstance>> GetAsync(string name)
        {
            return null;
        }

        public void Set(string name, IList<IServiceInstance> services, TimeSpan? slidingExpiration)
        {

        }

        public Task SetAsync(string name, IList<IServiceInstance> services, TimeSpan? slidingExpiration)
        {
#if NETSTANDARD
            return Task.CompletedTask;
#endif
#if NET45
            return Task.FromResult<object>(null);
#endif
        }
    }
}
