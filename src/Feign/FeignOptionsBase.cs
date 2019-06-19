using Feign.Formatting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Feign
{
    public abstract class FeignOptionsBase : IFeignOptions
    {
        protected FeignOptionsBase()
        {
            Converters = new ConverterCollection();
            Converters.AddConverter(new ObjectStringConverter());
            FeignClientPipeline = new GlobalFeignClientPipelineBuilder();
        }
        public ConverterCollection Converters { get; }
        public IGlobalFeignClientPipelineBuilder FeignClientPipeline { get; }
    }
}
