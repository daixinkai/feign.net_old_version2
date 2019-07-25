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
    public abstract class FallbackFeignClientHttpProxy<TService, TFallback> : FallbackFeignClientHttpProxy, IFallbackFeignClient<TService>, IFeignClient<TService> where TService : class
      where TFallback : TService
    {
        public FallbackFeignClientHttpProxy(IFeignOptions feignOptions, IServiceDiscovery serviceDiscovery, IServiceCacheProvider serviceCacheProvider, ILoggerFactory loggerFactory, TFallback fallback) : base(feignOptions, serviceDiscovery, serviceCacheProvider, loggerFactory, fallback)
        {
            Fallback = fallback;
            _serviceTypeFeignClientPipeline = _globalFeignClientPipeline?.GetServicePipeline<TService>();
        }
        public virtual TService Fallback { get; }

        TService IFeignClient<TService>.Service { get { return this as TService; } }

        ServiceTypeFeignClientPipeline<TService> _serviceTypeFeignClientPipeline;

        protected internal override void OnBuildingRequest(BuildingRequestEventArgs e)
        {
            _serviceTypeFeignClientPipeline?.OnBuildingRequest(this, e);
            base.OnBuildingRequest(e);
        }

        protected internal override void OnCancelRequest(CancelRequestEventArgs e)
        {
            _serviceTypeFeignClientPipeline?.OnCancelRequest(this, e);
            base.OnCancelRequest(e);
        }

        protected internal override void OnDisposing(DisposingEventArgs e)
        {
            _serviceTypeFeignClientPipeline?.OnDisposing(this, e);
            base.OnDisposing(e);
        }

        protected internal override void OnInitializing(InitializingEventArgs e)
        {
            _serviceTypeFeignClientPipeline?.OnInitializing(this, e);
            base.OnInitializing(e);
        }

        protected internal override void OnErrorRequest(ErrorRequestEventArgs e)
        {
            _serviceTypeFeignClientPipeline?.OnErrorRequest(this, e);
            base.OnErrorRequest(e);
        }
        protected internal override void OnReceivingResponse(ReceivingResponseEventArgs e)
        {
            _serviceTypeFeignClientPipeline?.OnReceivingResponse(this, e);
            base.OnReceivingResponse(e);
        }
        protected internal override void OnSendingRequest(SendingRequestEventArgs e)
        {
            _serviceTypeFeignClientPipeline?.OnSendingRequest(this, e);
            base.OnSendingRequest(e);
        }
        protected internal override void OnFallbackRequest(FallbackRequestEventArgs e)
        {
            _serviceTypeFeignClientPipeline?.OnFallbackRequest(this, e);
            base.OnFallbackRequest(e);
        }

    }

}
