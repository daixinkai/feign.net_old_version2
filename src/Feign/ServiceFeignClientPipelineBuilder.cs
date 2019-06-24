using System;
using System.Collections.Generic;
using System.Text;

namespace Feign
{
    class ServiceFeignClientPipelineBuilder : FeignClientPipelineBuilderBase, IServiceFeignClientPipelineBuilder
    {
        public ServiceFeignClientPipelineBuilder(string serviceId)
        {
            _serviceId = serviceId;
        }
        string _serviceId;
    }
}
