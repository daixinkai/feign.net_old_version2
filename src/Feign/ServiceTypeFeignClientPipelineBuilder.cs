using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Feign
{
    class ServiceTypeFeignClientPipelineBuilder : FeignClientPipelineBuilderBase, IServiceFeignClientPipelineBuilder
    {
        public ServiceTypeFeignClientPipelineBuilder(Type type)
        {
            _type = type;
        }
        Type _type;
    }
}
