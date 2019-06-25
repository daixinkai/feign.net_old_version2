using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace Feign
{
    class GlobalFeignClientPipelineBuilder : FeignClientPipelineBuilderBase, IGlobalFeignClientPipelineBuilder
    {
        IDictionary<string, ServiceFeignClientPipelineBuilder> _servicePipelineBuilderMap = new Dictionary<string, ServiceFeignClientPipelineBuilder>();
        public ServiceFeignClientPipelineBuilder GetServicePipeline(string serviceId)
        {
            ServiceFeignClientPipelineBuilder serviceFeignClientPipeline;
            _servicePipelineBuilderMap.TryGetValue(serviceId, out serviceFeignClientPipeline);
            return serviceFeignClientPipeline;
        }
        public ServiceFeignClientPipelineBuilder GetOrAddServicePipeline(string serviceId)
        {
            ServiceFeignClientPipelineBuilder serviceFeignClientPipeline;
            if (_servicePipelineBuilderMap.TryGetValue(serviceId, out serviceFeignClientPipeline))
            {
                return serviceFeignClientPipeline;
            }
            serviceFeignClientPipeline = new ServiceFeignClientPipelineBuilder(serviceId);
            _servicePipelineBuilderMap[serviceId] = serviceFeignClientPipeline;
            return serviceFeignClientPipeline;
        }


        public void InvokeBuildingRequest(IFeignClient feignClient, BuildingRequestEventArgs e)
        {
            GetServicePipeline(feignClient.ServiceId)?.OnBuildingRequest(feignClient, e);
            OnBuildingRequest(feignClient, e);
        }
        public void InvokeSendingRequest(IFeignClient feignClient, SendingRequestEventArgs e)
        {
            GetServicePipeline(feignClient.ServiceId)?.OnSendingRequest(feignClient, e);
            OnSendingRequest(feignClient, e);
        }
        public void InvokeCancelRequest(IFeignClient feignClient, CancelRequestEventArgs e)
        {
            GetServicePipeline(feignClient.ServiceId)?.OnCancelRequest(feignClient, e);
            OnCancelRequest(feignClient, e);
        }
        public void InvokeErrorRequest(IFeignClient feignClient, ErrorRequestEventArgs e)
        {
            GetServicePipeline(feignClient.ServiceId)?.OnErrorRequest(feignClient, e);
            if (!e.ExceptionHandled)
            {
                OnErrorRequest(this, e);
            }
        }
        public void InvokeReceivingResponse(IFeignClient feignClient, ReceivingResponseEventArgs e)
        {
            GetServicePipeline(feignClient.ServiceId)?.OnReceivingResponse(feignClient, e);
            OnReceivingResponse(feignClient, e);
        }
        public void InvokeInitializing(IFeignClient feignClient, InitializingEventArgs e)
        {
            GetServicePipeline(feignClient.ServiceId)?.OnInitializing(feignClient, e);
            OnInitializing(feignClient, e);
        }
        public void InvokeDisposing(IFeignClient feignClient, DisposingEventArgs e)
        {
            GetServicePipeline(feignClient.ServiceId)?.OnDisposing(feignClient, e);
            OnDisposing(feignClient, e);
        }
        public void InvokeFallbackRequest(IFeignClient feignClient, FallbackRequestEventArgs e)
        {
            GetServicePipeline(feignClient.ServiceId)?.OnFallbackRequest(feignClient, e);
            OnFallbackRequest(feignClient, e);
        }

        IServiceFeignClientPipelineBuilder IGlobalFeignClientPipelineBuilder.GetServicePipeline(string serviceId)
        {
            return GetServicePipeline(serviceId);
        }

        IServiceFeignClientPipelineBuilder IGlobalFeignClientPipelineBuilder.GetOrAddServicePipeline(string serviceId)
        {
            return GetOrAddServicePipeline(serviceId);
        }
    }
}
