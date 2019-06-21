using Feign.Cache;
using Feign.Discovery;
using Feign.Fallback;
using Feign.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Feign.Proxy
{
    public abstract class FallbackFeignClientProxyService<TService, TFallback> : FallbackFeignClientProxyService, IFallbackFeignClient<TService>
      where TFallback : TService
    {
        public FallbackFeignClientProxyService(IFeignOptions feignOptions, IServiceDiscovery serviceDiscovery, IServiceCacheProvider serviceCacheProvider, ILoggerFactory loggerFactory, TFallback fallback) : base(feignOptions, serviceDiscovery, serviceCacheProvider, loggerFactory)
        {
            Fallback = fallback;
        }
        public virtual TService Fallback { get; }
    }

}
