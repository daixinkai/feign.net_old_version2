﻿using Feign.Cache;
using Feign.Discovery;
using Feign.Fallback;
using Feign.Logging;
using Feign.Proxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Feign.Standalone.Proxy
{
    public abstract class StandaloneFallbackFactoryFeignClientProxyService<TService, TFallbackFactory> : FallbackFactoryFeignClientProxyService<TService, TFallbackFactory>
        where TFallbackFactory : IFallbackFactory<TService>
    {
        public StandaloneFallbackFactoryFeignClientProxyService() : base(GetService<IFeignOptions>(), GetService<IServiceDiscovery>(), GetService<IServiceCacheProvider>(), GetService<ILoggerFactory>(), GetService<TFallbackFactory>())
        {
        }

        protected static T GetService<T>()
        {
            return FeignClients._standaloneFeignBuilder.GetService<T>();
        }

    }
}
