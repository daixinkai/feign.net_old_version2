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
    //public abstract class FallbackFeignClientProxyService<TService, TFallback, TFallbackFactory> : FeignClientProxyService<TService>
    //      where TFallback : TService
    //{
    //    public FallbackFeignClientProxyService(IFeignOptions feignOptions, IServiceDiscovery serviceDiscovery, IServiceCacheProvider serviceCacheProvider, ILoggerFactory loggerFactory, TFallback fallback, TFallbackFactory fallbackFactory) : base(feignOptions, serviceDiscovery, serviceCacheProvider, loggerFactory)
    //    {
    //        _fallback = fallback;
    //        _fallbackFactory = fallbackFactory;
    //    }

    //    TFallback _fallback;
    //    TFallbackFactory _fallbackFactory;
    //}

    public abstract class FallbackFeignClientProxyService<TService, TFallback> : FallbackFeignClientProxyService, IFallbackFeignClient<TService>
      where TFallback : TService
    {
        public FallbackFeignClientProxyService(IFeignOptions feignOptions, IServiceDiscovery serviceDiscovery, IServiceCacheProvider serviceCacheProvider, ILoggerFactory loggerFactory, TFallback fallback) : base(feignOptions, serviceDiscovery, serviceCacheProvider, loggerFactory)
        {
            Fallback = fallback;
        }
        public virtual TService Fallback { get; }
    }

    public abstract class FallbackFactoryFeignClientProxyService<TService, TFallbackFactory> : FallbackFeignClientProxyService<TService, TService>, IFallbackFactoryFeignClient<TService>
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
