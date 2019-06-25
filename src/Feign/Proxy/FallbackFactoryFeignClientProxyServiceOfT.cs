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
    public abstract class FallbackFactoryFeignClientProxyService<TService, TFallbackFactory> : FallbackFeignClientProxyService<TService, TService>, IFallbackFactoryFeignClient<TService>, IFeignClient<TService>
        where TFallbackFactory : IFallbackFactory<TService>
    {
        public FallbackFactoryFeignClientProxyService(IFeignOptions feignOptions, IServiceDiscovery serviceDiscovery, IServiceCacheProvider serviceCacheProvider, ILoggerFactory loggerFactory, TFallbackFactory fallbackFactory) : base(feignOptions, serviceDiscovery, serviceCacheProvider, loggerFactory, GetFallback(fallbackFactory))
        {
            FallbackFactory = fallbackFactory;
        }

        public TFallbackFactory FallbackFactory { get; }

        IFallbackFactory<TService> IFallbackFactoryFeignClient<TService>.FallbackFactory
        {
            get
            {
                return FallbackFactory;
            }
        }


        static TService GetFallback(IFallbackFactory<TService> fallbackFactory)
        {
            if (fallbackFactory != null)
            {
                return fallbackFactory.GetFallback();
            }
            return default(TService);
        }

        protected override void Dispose(bool disposing)
        {
            if (FallbackFactory != null && Fallback != null)
            {
                FallbackFactory.ReleaseFallback(Fallback);
            }
            base.Dispose(disposing);
        }

    }

}
