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
    public abstract class StandaloneFallbackFeignClientHttpProxy<TService, TFallback> : FallbackFeignClientHttpProxy<TService, TFallback>
      where TFallback : TService
    {
        public StandaloneFallbackFeignClientHttpProxy() : base(GetService<IFeignOptions>(), GetService<IServiceDiscovery>(), GetService<IServiceCacheProvider>(), GetService<ILoggerFactory>(), GetService<TFallback>())
        {
        }

        protected static T GetService<T>()
        {
            return FeignClients._standaloneFeignBuilder.GetService<T>();
        }

    }

}
