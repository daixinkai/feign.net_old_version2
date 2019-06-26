using System;
using System.Collections.Generic;
using System.Text;

namespace Feign
{
    class ServiceIdFeignClientPipeline : FeignClientPipelineBase, IServiceFeignClientPipeline
    {
        public ServiceIdFeignClientPipeline(string serviceId)
        {
            ServiceId = serviceId;
        }
        public string ServiceId { get; }
    }
}
