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
            _method = method;
        }
        public FeignClientRequest Request { get; }
        public IFallbackProxy FallbackProxy { get; }
        public object Fallback { get; }
        MethodInfo _method;
        public MethodInfo Method
        {
            get
            {
                if (_method == null)
                {
                    _method = Fallback.GetType().GetMethod(FallbackProxy.MethodName, FallbackProxy.GetParameterTypes());
                }
                return _method;
            }
        }

        public IDictionary<string, object> GetParameters()
        {
            return FallbackProxy?.GetParameters() ?? new Dictionary<string, object>();
        }

        public Type[] GetParameterTypes()
        {
            return FallbackProxy?.GetParameterTypes() ?? Type.EmptyTypes;
        }

        public bool IsTerminated => _isTerminated;

        bool _isTerminated;

        public void Terminate()
        {
            _isTerminated = true;
        }

    }
}
