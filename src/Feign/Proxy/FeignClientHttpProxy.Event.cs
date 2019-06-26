using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Feign.Proxy
{
    partial class FeignClientHttpProxy
    {

        protected internal virtual void OnBuildingRequest(BuildingRequestEventArgs e)
        {
            _serviceIdFeignClientPipeline?.OnBuildingRequest(this, e);
            _globalFeignClientPipeline?.OnBuildingRequest(this, e);
        }
        protected internal virtual void OnSendingRequest(SendingRequestEventArgs e)
        {
            _serviceIdFeignClientPipeline?.OnSendingRequest(this, e);
            _globalFeignClientPipeline?.OnSendingRequest(this, e);
        }
        protected internal virtual void OnCancelRequest(CancelRequestEventArgs e)
        {
            _serviceIdFeignClientPipeline?.OnCancelRequest(this, e);
            _globalFeignClientPipeline?.OnCancelRequest(this, e);
        }
        protected internal virtual void OnErrorRequest(ErrorRequestEventArgs e)
        {
            _serviceIdFeignClientPipeline?.OnErrorRequest(this, e);
            _globalFeignClientPipeline?.OnErrorRequest(this, e);
        }
        protected internal virtual void OnReceivingResponse(ReceivingResponseEventArgs e)
        {
            _serviceIdFeignClientPipeline?.OnReceivingResponse(this, e);
            _globalFeignClientPipeline?.OnReceivingResponse(this, e);
        }
        protected internal virtual void OnInitializing(InitializingEventArgs e)
        {
            _serviceIdFeignClientPipeline?.OnInitializing(this, e);
            _globalFeignClientPipeline?.OnInitializing(this, e);
        }
        protected internal virtual void OnDisposing(DisposingEventArgs e)
        {
            _serviceIdFeignClientPipeline?.OnDisposing(this, e);
            _globalFeignClientPipeline?.OnDisposing(this, e);
        }

    }
}
