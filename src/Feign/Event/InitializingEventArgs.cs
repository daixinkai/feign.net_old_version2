using Feign.Cache;
using Feign.Discovery;
using Feign.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Feign
{
    public class InitializingEventArgs : FeignClientEventArgs
    {
        public InitializingEventArgs(IFeignClient feignClient) : base(feignClient)
        {
        }

        public HttpClient HttpClient { get; set; }

    }
}
