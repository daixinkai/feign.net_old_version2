using Feign.Fallback;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Feign
{
    public class FallbackRequestEventArgs : FeignClientEventArgs
    {
        public FallbackRequestEventArgs(IFeignClient feignClient, FeignClientRequest request, object fallback, IFallbackProxy fallbackProxy, MethodInfo method) : base(feignClient)
        {
            Request = request;
            Fallback = fallback;
            FallbackProxy = fallbackProxy;
            Method = method;
        }
        public FeignClientRequest Request { get; }
        public IFallbackProxy FallbackProxy { get; }
        public object Fallback { get; }
        public MethodInfo Method { get; }

        public IDictionary<string, object> GetParameters()
        {
            if (FallbackProxy != null)
            {
                return FallbackProxy.GetParameters();
            }
            return new Dictionary<string, object>();
        }

    }
}
