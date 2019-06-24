using System;
using System.Collections.Generic;
using System.Text;

namespace Feign
{
    public interface IGlobalFeignClientPipelineBuilder : IFeignClientPipelineBuilder
    {
        IServiceFeignClientPipelineBuilder GetServicePipeline(string serviceId);
        IServiceFeignClientPipelineBuilder GetOrAddServicePipeline(string serviceId);
    }
}
