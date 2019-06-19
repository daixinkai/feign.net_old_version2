using Feign.Formatting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Feign
{
    public interface IFeignOptions
    {
        ConverterCollection Converters { get; }
        IGlobalFeignClientPipelineBuilder FeignClientPipeline { get; }
    }
}
