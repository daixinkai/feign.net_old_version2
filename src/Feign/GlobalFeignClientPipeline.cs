using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace Feign
{
    class GlobalFeignClientPipeline : FeignClientPipelineBase, IGlobalFeignClientPipeline
    {
        IDictionary<string, ServiceIdFeignClientPipeline> _serviceIdPipelineMap = new Dictionary<string, ServiceIdFeignClientPipeline>();
        IDictionary<Type, ServiceTypeFeignClientPipeline> _serviceTypePipelineMap = new Dictionary<Type, ServiceTypeFeignClientPipeline>();
        public ServiceIdFeignClientPipeline GetServicePipeline(string serviceId)
        {
            ServiceIdFeignClientPipeline serviceFeignClientPipeline;
            _serviceIdPipelineMap.TryGetValue(serviceId, out serviceFeignClientPipeline);
            return serviceFeignClientPipeline;
        }
        public ServiceIdFeignClientPipeline GetOrAddServicePipeline(string serviceId)
        {
            ServiceIdFeignClientPipeline serviceFeignClientPipeline;
            if (_serviceIdPipelineMap.TryGetValue(serviceId, out serviceFeignClientPipeline))
            {
                return serviceFeignClientPipeline;
            }
            serviceFeignClientPipeline = new ServiceIdFeignClientPipeline(serviceId);
            _serviceIdPipelineMap[serviceId] = serviceFeignClientPipeline;
            return serviceFeignClientPipeline;
        }

        public ServiceTypeFeignClientPipeline GetServicePipeline<TService>()
        {
            ServiceTypeFeignClientPipeline serviceFeignClientPipeline;
            _serviceTypePipelineMap.TryGetValue(typeof(TService), out serviceFeignClientPipeline);
            return serviceFeignClientPipeline;
        }
        public ServiceTypeFeignClientPipeline GetOrAddServicePipeline<TService>()
        {
            ServiceTypeFeignClientPipeline serviceFeignClientPipeline;
            if (_serviceTypePipelineMap.TryGetValue(typeof(TService), out serviceFeignClientPipeline))
            {
                return serviceFeignClientPipeline;
            }
            serviceFeignClientPipeline = new ServiceTypeFeignClientPipeline(typeof(TService));
            _serviceTypePipelineMap[typeof(TService)] = serviceFeignClientPipeline;
            return serviceFeignClientPipeline;
        }

        IServiceFeignClientPipeline IGlobalFeignClientPipeline.GetServicePipeline(string serviceId)
        {
            return GetServicePipeline(serviceId);
        }

        IServiceFeignClientPipeline IGlobalFeignClientPipeline.GetOrAddServicePipeline(string serviceId)
        {
            return GetOrAddServicePipeline(serviceId);
        }
        IServiceFeignClientPipeline IGlobalFeignClientPipeline.GetServicePipeline<TService>()
        {
            return GetServicePipeline<TService>();
        }

        IServiceFeignClientPipeline IGlobalFeignClientPipeline.GetOrAddServicePipeline<TService>()
        {
            return GetOrAddServicePipeline<TService>();
        }
    }
}
