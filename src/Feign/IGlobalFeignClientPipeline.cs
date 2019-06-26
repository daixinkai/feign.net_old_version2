using System;
using System.Collections.Generic;
using System.Text;

namespace Feign
{
    public interface IGlobalFeignClientPipeline : IFeignClientPipeline
    {
        IServiceFeignClientPipeline GetServicePipeline(string serviceId);
        IServiceFeignClientPipeline GetOrAddServicePipeline(string serviceId);
        IServiceFeignClientPipeline GetServicePipeline<TService>();
        IServiceFeignClientPipeline GetOrAddServicePipeline<TService>();
    }
}
