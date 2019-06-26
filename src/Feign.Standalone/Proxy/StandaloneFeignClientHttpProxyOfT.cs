﻿using Feign.Cache;
using Feign.Discovery;
using Feign.Logging;
using Feign.Proxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Feign.Standalone.Proxy
{
    public abstract class StandaloneFeignClientHttpProxy<TService> : FeignClientHttpProxy<TService>
    {
        public StandaloneFeignClientHttpProxy() : base(GetService<IFeignOptions>(), GetService<IServiceDiscovery>(), GetService<IServiceCacheProvider>(), GetService<ILoggerFactory>())
        {
        }

        protected static T GetService<T>()
        {
            return FeignClients._standaloneFeignBuilder.GetService<T>();
        }

    }
}
