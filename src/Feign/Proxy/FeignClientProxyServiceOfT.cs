using Feign.Cache;
using Feign.Discovery;
using Feign.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Feign.Proxy
{
    public abstract class FeignClientProxyService<TService> : FeignClientProxyService
    {
        public FeignClientProxyService(IFeignOptions feignOptions, IServiceDiscovery serviceDiscovery, IServiceCacheProvider serviceCacheProvider, ILoggerFactory loggerFactory) : base(feignOptions, serviceDiscovery, serviceCacheProvider, loggerFactory)
        {
        }
    }
}
