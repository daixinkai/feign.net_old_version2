﻿using Feign.Cache;
using Feign.Internal;
using Feign.Logging;
using Feign.Proxy;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Feign.Discovery
{
    public class ServiceDiscoveryHttpClientHandler : FeignProxyHttpClientHandler
    {

        private IServiceResolve _serviceResolve;
        private IServiceDiscovery _serviceDiscovery;
        private IServiceCacheProvider _serviceCacheProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceDiscoveryHttpClientHandler"/> class.
        /// </summary>
        public ServiceDiscoveryHttpClientHandler(FeignClientHttpProxy feignClient, IServiceDiscovery serviceDiscovery, IServiceCacheProvider serviceCacheProvider, ILogger logger) : base(feignClient, logger)
        {
            _serviceResolve = new RandomServiceResolve(logger);
            _serviceDiscovery = serviceDiscovery;
            _serviceCacheProvider = serviceCacheProvider;
            ShouldResolveService = true;
        }


        public bool ShouldResolveService { get; set; }


        protected override Uri LookupRequestUri(Uri uri)
        {
            if (!ShouldResolveService)
            {
                return uri;
            }
            if (_serviceDiscovery == null)
            {
                return uri;
            }
            IList<IServiceInstance> services = _serviceDiscovery.GetServiceInstancesWithCache(uri.Host, _serviceCacheProvider);
            return _serviceResolve.ResolveService(uri, services);
        }

    }
}
