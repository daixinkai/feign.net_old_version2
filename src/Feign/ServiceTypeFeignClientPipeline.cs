using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Feign
{
    class ServiceTypeFeignClientPipeline : FeignClientPipelineBase, IServiceFeignClientPipeline
    {
        public ServiceTypeFeignClientPipeline(Type serviceType)
        {
            ServiceType = serviceType;
        }
        public Type ServiceType { get; }
    }

    class ServiceTypeFeignClientPipeline<TService> : ServiceTypeFeignClientPipeline, IServiceFeignClientPipeline<TService>
    {
        public ServiceTypeFeignClientPipeline() : base(typeof(TService))
        {
        }
    }

}
