using Feign.Formatting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Feign
{
    public class FeignOptions : IFeignOptions
    {
        public FeignOptions()
        {
            Assemblies = new List<Assembly>();
            Converters = new ConverterCollection();
            Converters.AddConverter(new ObjectStringConverter());
            FeignClientPipeline = new GlobalFeignClientPipelineBuilder();
            Lifetime = FeignClientLifetime.Transient;
        }
        public IList<Assembly> Assemblies { get; }
        public ConverterCollection Converters { get; }
        public IGlobalFeignClientPipelineBuilder FeignClientPipeline { get; }
        public FeignClientLifetime Lifetime { get; set; }
    }
}
