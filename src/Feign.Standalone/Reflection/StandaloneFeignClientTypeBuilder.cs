using Feign.Fallback;
using Feign.Proxy;
using Feign.Reflection;
using Feign.Standalone.Proxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Feign.Standalone.Reflection
{
    class StandaloneFeignClientTypeBuilder : FeignClientTypeBuilder
    {
        protected override Type GetParentType(Type parentType)
        {
            if (typeof(FallbackFactoryFeignClientProxyService<,>) == parentType.GetGenericTypeDefinition())
            {
                return typeof(StandaloneFallbackFactoryFeignClientProxyService<,>).MakeGenericType(parentType.GetGenericArguments());
            }
            if (typeof(FallbackFeignClientProxyService<,>) == parentType.GetGenericTypeDefinition())
            {
                return typeof(StandaloneFallbackFeignClientProxyService<,>).MakeGenericType(parentType.GetGenericArguments());
            }
            return typeof(StandaloneFeignClientProxyService<>).MakeGenericType(parentType.GetGenericArguments());
        }
    }
}
