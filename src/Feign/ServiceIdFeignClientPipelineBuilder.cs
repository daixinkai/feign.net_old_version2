using System;
using System.Collections.Generic;
using System.Text;

namespace Feign
{
    class ServiceIdFeignClientPipelineBuilder : FeignClientPipelineBuilderBase, IServiceFeignClientPipelineBuilder
    {
        public ServiceIdFeignClientPipelineBuilder(string serviceId)
        {
            _serviceId = serviceId;
        }
        string _serviceId;
    }
}
