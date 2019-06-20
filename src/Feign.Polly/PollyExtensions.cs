using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Feign
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class PollyExtensions
    {

        //public static IFeignClientPipelineBuilder AddPolly(this IGlobalFeignClientPipelineBuilder globalFeignClientPipelineBuilder)
        //{
        //    if (string.IsNullOrWhiteSpace(serviceId))
        //    {
        //        throw new ArgumentException(nameof(serviceId));
        //    }
        //    //IGlobalFeignClientPipelineBuilder pipelineBuilder = globalFeignClientPipelineBuilder as IGlobalFeignClientPipelineBuilder;
        //    //if (pipelineBuilder == null)
        //    //{
        //    //    throw new NotSupportedException();
        //    //}
        //    //return pipelineBuilder.GetOrAddServicePipeline(serviceId);

        //    globalFeignClientPipelineBuilder.SendingRequest += GlobalFeignClientPipelineBuilder_SendingRequest;

        //}

        //private static void GlobalFeignClientPipelineBuilder_SendingRequest(object sender, SendingRequestEventArgs e)
        //{
            
        //}

    }
}
